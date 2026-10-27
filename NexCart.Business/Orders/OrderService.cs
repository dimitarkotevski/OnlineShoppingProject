using System;
using NexCart.Data.NHibernate.Repositories;
using NexCart.Data.NHibernate.ConfigurationRepository;
using System.Linq;
using NexCart.Business.Exceptions;
using NexCart.Business.Exceptions.ExceptionMassages;
using NexCart.Domain;
using NexCart.Business.Requests;
using NexCart.Business.Responses;
using NexCart.Business.ViewModels;
using NexCart.Business.Mappers;
using NexCart.Domain.Enums;
using NexCart.Domain.Enum;
using System.Collections.Generic;

namespace NexCart.Business.Services
{
    public interface IOrderService
    {
        ResponseModel<OrderViewModel> GetAll(RequestModel request);
        OrderViewModel Get(Guid Id);
        Guid Create(OrderViewModel model);
        void Update(OrderViewModel model);
        void Delete(Guid Id);
        void DeleteMany(IEnumerable<Guid> ids);
        void ChangeOrderStatus(Guid orderId, OrderStatus orderStatus);
        ResponseListModel<OrderGridViewModel> GetUserNotActiveOrders(Guid userId);
        OrderViewModelWithProducts GetUserActiveOrder(Guid userId);
        OrderViewModelWithProducts GetUserOrder(Guid orderId, Guid userId);
        void CancelActiveOrder(Guid orderId);
        List<OrderViewModelWithProducts> GetUserOtherOrders(Guid userId);
        void MakeAnOrder(Guid shoppingCartId,Guid userId);
        OrderStatus MakeAPay(OrderViewModel order);
        bool DoesOrderBelongToUser(Guid orderId, Guid userId);

    }
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly IProductRepository _productRepository;

        public OrderService(IOrderRepository orderRepository,
                            IUserRepository userRepository,
                            IProductRepository productRepository,
                            IUnitOfWork unitOfWork,
                            IShoppingCartRepository shoppingCartRepository)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _shoppingCartRepository = shoppingCartRepository;
        }

        #region CRUD ADMIN

        public Guid Create(OrderViewModel model)
        {
            if (model?.User == null || model.ShoppingCart == null)
            {
                throw new BllException(ShopExceptionMassages.OrderExceptionMassages.NOT_FOUND_EXCEPTION);
            }

            using (_unitOfWork.BeginTransaction())
            {
                var user = _userRepository.FindBy(model.User.Id);
                if(user == null)
                {
                    throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);
                }
                var shoppingCart = _shoppingCartRepository.FindBy(model.ShoppingCart.Id);
                if(shoppingCart == null)
                {
                    throw new BllException(ShopExceptionMassages.ShoppingCartExceptionMassages.NOT_FOUNG);
                }
                var order = new Order
                {
                    User = user,
                    ShoppingCart = shoppingCart,
                    OrderStatus = model.OrderStatus,
                    PaymentType = model.PaymentType,
                    OrderName = string.IsNullOrWhiteSpace(model.OrderName) ? NewOrderName() : model.OrderName.Trim(),
                    DateCreated = DateTime.UtcNow,
                    TotalAmount = model.TotalAmount
                };
                _orderRepository.Add(order);
                _unitOfWork.Commit();
                return order.Id;
            }
        }
        public void Delete(Guid Id)
        {
            using (_unitOfWork.BeginTransaction())
            {
                _orderRepository.Delete(Id);
                _unitOfWork.Commit();
            }
        }
        /// <summary>Deletes all the given orders in one transaction (all or nothing).</summary>
        public void DeleteMany(IEnumerable<Guid> ids)
        {
            var distinctIds = (ids ?? Enumerable.Empty<Guid>()).Distinct().ToList();
            if (distinctIds.Count == 0)
            {
                return;
            }

            using (_unitOfWork.BeginTransaction())
            {
                foreach (var id in distinctIds)
                {
                    _orderRepository.Delete(id);
                }
                _unitOfWork.Commit();
            }
        }
        public OrderViewModel Get(Guid Id)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var orderEntity=_orderRepository.FindBy(Id);
                if (orderEntity == null)
                {
                    throw new BllException(ShopExceptionMassages.OrderExceptionMassages.NOT_FOUND_EXCEPTION);
                }
                _unitOfWork.Commit();
                return orderEntity.MapToViewModel();
            }
        }
        public ResponseModel<OrderViewModel> GetAll(RequestModel request)
        {
            return FilterAndPage(request);
        }
        private ResponseModel<OrderViewModel> FilterAndPage(RequestModel request)
        {
            using (_unitOfWork.BeginTransaction())
            {
                ResponseModel<OrderViewModel> response = new ResponseModel<OrderViewModel>();
                IQueryable<Order> ordersListForFiltering = _orderRepository.All();

                if (!string.IsNullOrEmpty(request.SearchString))
                {
                    ordersListForFiltering = ordersListForFiltering
                                        .Where(x => x.User.FirstName.Contains(request.SearchString)  || 
                                                    x.User.LastName.Contains(request.SearchString) || 
                                                    x.User.Username.Contains(request.SearchString) ||
                                                    x.OrderName.Contains(request.SearchString) || 
                                                    (x.ShoppingCart != null && x.ShoppingCart.Name.Contains(request.SearchString)) ||
                                                    x.OrderItems.Any(item => item.ProductName.Contains(request.SearchString)));
                }

                if (request.OrderStatus.HasValue)
                {
                    var status = request.OrderStatus.Value;
                    ordersListForFiltering = ordersListForFiltering.Where(x => x.OrderStatus == status);
                }
                if (request.PaymentType.HasValue)
                {
                    var paymentType = request.PaymentType.Value;
                    ordersListForFiltering = ordersListForFiltering.Where(x => x.PaymentType == paymentType);
                }
                if (request.PriceFrom.HasValue)
                {
                    var minTotal = request.PriceFrom.Value;
                    ordersListForFiltering = ordersListForFiltering.Where(x => x.TotalAmount >= minTotal);
                }
                if (request.PriceTo.HasValue)
                {
                    var maxTotal = request.PriceTo.Value;
                    ordersListForFiltering = ordersListForFiltering.Where(x => x.TotalAmount <= maxTotal);
                }
                if (request.DateFrom.HasValue)
                {
                    var from = request.DateFrom.Value.Date;
                    ordersListForFiltering = ordersListForFiltering.Where(x => x.DateCreated >= from);
                }
                if (request.DateTo.HasValue)
                {
                    var toExclusive = request.DateTo.Value.Date.AddDays(1);
                    ordersListForFiltering = ordersListForFiltering.Where(x => x.DateCreated < toExclusive);
                }


                switch (request.SortName)
                {
                    case "ProductName":
                        if (string.IsNullOrEmpty(request.isAscending))
                        {
                            break;
                        }
                        else if (request.isAscending.Equals("asc"))
                        {
                            ordersListForFiltering = ordersListForFiltering.OrderBy(x => x.OrderItems.Min(item => item.ProductName));
                        }
                        else
                        {
                            ordersListForFiltering = ordersListForFiltering.OrderByDescending(x => x.OrderItems.Min(item => item.ProductName));
                        }
                        break;
                    case "UserName":
                        if (string.IsNullOrEmpty(request.isAscending))
                        {
                            break;
                        }
                        else if (request.isAscending.Equals("asc"))
                        {
                            ordersListForFiltering = ordersListForFiltering.OrderBy(x => x.User.FirstName);
                        }
                        else
                        {
                            ordersListForFiltering = ordersListForFiltering.OrderByDescending(x => x.User.FirstName);
                        }
                        break;
                    case "Quantity":
                        if (string.IsNullOrEmpty(request.isAscending))
                        {
                            break;
                        }
                        else if (request.isAscending.Equals("asc"))
                        {
                            ordersListForFiltering = ordersListForFiltering.OrderBy(x => x.OrderItems.Sum(item => item.Quantity));
                        }
                        else
                        {
                            ordersListForFiltering = ordersListForFiltering.OrderByDescending(x => x.OrderItems.Sum(item => item.Quantity));
                        }
                        break;
                    default:
                        break;
                }
                if (request.Size == 0 && request.PageNeeded == 0)
                {
                    request.PageNeeded = 1;
                    request.Size = ordersListForFiltering.Count();
                }

                response.Size = request.Size;
                response.CurrentPage = request.PageNeeded;
                response.Items = ordersListForFiltering
                                    .Skip((response.CurrentPage - 1) * response.Size)
                                    .Take(response.Size).Select(x => x.MapToViewModel()).ToList();
                response.TotalItems = ordersListForFiltering.Count();

                return response;
            }
        }
        public void Update(OrderViewModel model)
        {
            using(_unitOfWork.BeginTransaction())
            {
                var entityOrder = _orderRepository.FindBy(model.Id);
                if (entityOrder == null)
                {
                    throw new BllException(ShopExceptionMassages.OrderExceptionMassages.NOT_FOUND_EXCEPTION);
                }
                var user = _userRepository.FindBy(model.User.Id);
                if(user == null)
                {
                    throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);
                }
                
                entityOrder.OrderStatus = model.OrderStatus;
                entityOrder.PaymentType = model.PaymentType;
                
                _orderRepository.Update(entityOrder);
                _unitOfWork.Commit();
            }
        }
        #endregion

        #region USER

        public ResponseListModel<OrderGridViewModel> GetUserNotActiveOrders(Guid userId)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var user = _userRepository.FindBy(userId);
                if(user == null)
                {
                    throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);
                }

                var userOrders = _orderRepository
                    .FilterBy(x => x.User == user && x.OrderStatus != OrderStatus.OnHold)
                    .OrderByDescending(x => x.DateCreated);

                return new ResponseListModel<OrderGridViewModel>
                {
                    Items = userOrders.Select(x => x.MapToGridViewModel()).ToList(),
                    TotalItems = userOrders.Count()
                };
            }
        }

        public OrderViewModelWithProducts GetUserActiveOrder(Guid userId)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var userOrder = _orderRepository
                    .FilterBy(x => x.User.Id == userId && x.OrderStatus == OrderStatus.OnHold)
                    .FirstOrDefault();

                // A customer with no active order is a normal state, not an error.
                if (userOrder == null)
                {
                    return null;
                }

                // Do not recalculate from the cart: OrderItems is the checkout snapshot.
                return userOrder.MapToViewModelWithProducts();
            }
        }

        /// <summary>
        /// Returns one order with its snapshot lines only when it belongs to the given user.
        /// The user id always comes from the authenticated caller, never from the request.
        /// </summary>
        public OrderViewModelWithProducts GetUserOrder(Guid orderId, Guid userId)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var order = _orderRepository
                    .FilterBy(x => x.Id == orderId && x.User.Id == userId)
                    .FirstOrDefault();

                return order?.MapToViewModelWithProducts();
            }
        }

        public List<OrderViewModelWithProducts> GetUserOtherOrders(Guid userId)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var user = _userRepository.FindBy(userId);
                if(user == null)
                {
                    throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);
                }

                var userOrders = _orderRepository.FilterBy(x => x.User.Id == userId && x.OrderStatus == OrderStatus.Completed);

                return userOrders.Select(x=>x.MapToViewModelWithProducts()).ToList();
            }
        }

        /// <summary>True when the order exists and belongs to the given user.</summary>
        public bool DoesOrderBelongToUser(Guid orderId, Guid userId)
        {
            using (_unitOfWork.BeginTransaction())
            {
                return _orderRepository.CheckExist(x => x.Id == orderId && x.User.Id == userId);
            }
        }

        public void CancelActiveOrder(Guid orderId)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var order = _orderRepository.FindBy(orderId)
                    ?? throw new BllException(ShopExceptionMassages.OrderExceptionMassages.NOT_FOUND_EXCEPTION);

                _orderRepository.Delete(order);
                _unitOfWork.Commit();
            }
        }

        /// <summary>
        /// Creates or refreshes the active order for a cart, copying every cart line into an
        /// immutable <see cref="OrderItem"/> snapshot and fixing the total at that moment.
        /// </summary>
        public void MakeAnOrder(Guid shoppingCartId,Guid userId)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var shoppingCart = _shoppingCartRepository.FindBy(shoppingCartId)
                    ?? throw new BllException(ShopExceptionMassages.ShoppingCartExceptionMassages.NOT_FOUNG);

                if (!shoppingCart.Active)
                {
                    throw new BllException(ShopExceptionMassages.ShoppingCartExceptionMassages.NOT_ACTIVE_EXCEPTION);
                }
                if (shoppingCart.User == null || shoppingCart.User.Id != userId)
                {
                    throw new BllException(ShopExceptionMassages.ShoppingCartExceptionMassages.NOT_FOUNG);
                }
                if (shoppingCart.ShoppingCartDetails == null || !shoppingCart.ShoppingCartDetails.Any())
                {
                    throw new BllException(ShopExceptionMassages.ShoppingCartExceptionMassages.EMPTY_EXCEPTION);
                }

                var user = _userRepository.FindBy(userId)
                    ?? throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);

                var order = _orderRepository.FindBy(x => x.ShoppingCart.Id == shoppingCartId
                                                        && x.OrderStatus == OrderStatus.OnHold);

                var isNewOrder = order == null;
                if (isNewOrder)
                {
                    order = new Order
                    {
                        OrderName = NewOrderName()
                    };
                }

                ReplaceOrderItems(order, shoppingCart);

                order.ShoppingCart = shoppingCart;
                order.User = user;
                order.DateCreated = DateTime.UtcNow;
                order.TotalAmount = order.OrderItems.Sum(item => item.UnitPrice * item.Quantity);
                order.OrderStatus = OrderStatus.OnHold;
                order.PaymentType = PaymentType.Cash;

                if (isNewOrder)
                {
                    _orderRepository.Add(order);
                }
                else
                {
                    _orderRepository.Update(order);
                }

                _unitOfWork.Commit();
            }
        }

        public void ChangeOrderStatus(Guid orderId, OrderStatus orderStatus)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var order = _orderRepository.FindBy(orderId);
                if(order == null)
                {
                    throw new BllException(ShopExceptionMassages.OrderExceptionMassages.NOT_FOUND_EXCEPTION);
                }

                order.OrderStatus = orderStatus;

                _orderRepository.Update(order);
                _unitOfWork.Commit();
            }
        }

        public OrderStatus MakeAPay(OrderViewModel model)
        {
            if (model == null)
            {
                throw new BllException(ShopExceptionMassages.OrderExceptionMassages.NOT_FOUND_EXCEPTION);
            }

            using (_unitOfWork.BeginTransaction())
            {
                var order = _orderRepository.FindBy(model.Id);
                if(order == null)
                {
                    throw new BllException(ShopExceptionMassages.OrderExceptionMassages.NOT_FOUND_EXCEPTION);
                }

                var user = order.User;
                if (user == null)
                {
                    throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);
                }

                var shoppingCart = order.ShoppingCart
                    ?? _shoppingCartRepository.FindBy(x => x.User.Id == user.Id && x.Active);

                if (shoppingCart == null)
                {
                    throw new BllException(ShopExceptionMassages.ShoppingCartExceptionMassages.NOT_FOUNG);
                }

                if (model.PaymentType == PaymentType.Cash)
                {
                    order.PaymentType = PaymentType.Cash;
                    order.OrderStatus = OrderStatus.PendingPayment;

                    DeactivateCartAndOpenNewOne(shoppingCart, user);

                    _orderRepository.Update(order);
                    _unitOfWork.Commit();
                }
                else if(model.PaymentType == PaymentType.Card)
                {
                    if(order.TotalAmount > user.Amount)
                    {
                        order.PaymentType = PaymentType.Card;
                        order.OrderStatus = OrderStatus.PendingPayment;

                        _orderRepository.Update(order);
                        _unitOfWork.Commit();
                    }
                    else
                    {
                        user.Amount = user.Amount - order.TotalAmount;

                        order.OrderStatus = OrderStatus.Completed;
                        order.PaymentType = PaymentType.Card; 

                        DeactivateCartAndOpenNewOne(shoppingCart, user);

                        _orderRepository.Update(order);
                        _userRepository.Update(user);
                        _unitOfWork.Commit();
                    }
                }
                else
                {
                    // DO NOTHING
                }

                return order.OrderStatus;
            }
        }

        #endregion

        /// <summary>
        /// Copies the current cart lines into order items. Existing items are replaced when the
        /// customer deliberately re-confirms the same cart.
        /// </summary>
        private static void ReplaceOrderItems(Order order, ShoppingCart shoppingCart)
        {
            if (order.OrderItems == null)
            {
                order.OrderItems = new List<OrderItem>();
            }

            order.OrderItems.Clear();

            foreach (var detail in shoppingCart.ShoppingCartDetails)
            {
                if (detail.Product == null)
                {
                    throw new BllException(ShopExceptionMassages.ProductsExceptionMassages.NOT_FOUND_EXCEPTION);
                }

                order.OrderItems.Add(new OrderItem
                {
                    Order = order,
                    Product = detail.Product,
                    ProductName = detail.Product.Name ?? string.Empty,
                    UnitPrice = detail.Product.Price,
                    Quantity = detail.Quantity
                });
            }
        }

        /// <summary>
        /// Closes the paid cart and creates the user's next empty active cart. The old cart is
        /// kept because it is the traceability link from the order.
        /// </summary>
        private void DeactivateCartAndOpenNewOne(ShoppingCart shoppingCart, User user)
        {
            shoppingCart.Active = false;

            var newShoppingCartForUser = new ShoppingCart
            {
                User = user,
                Active = true,
                Name = "Cart#" + Guid.NewGuid().ToString("N").Substring(0, 8)
            };

            // UX_ShoppingCarts_ActiveUser allows one active cart per user. NHibernate flushes
            // inserts before updates, so persist the deactivation before inserting the new one.
            _shoppingCartRepository.Update(shoppingCart);
            _unitOfWork.Session.Flush();
            _shoppingCartRepository.Add(newShoppingCartForUser);
        }

        private static string NewOrderName()
        {
            return "Order#" + Guid.NewGuid().ToString("N").Substring(0, 8);
        }
    }
}
