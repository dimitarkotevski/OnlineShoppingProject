using System;
using NexCart.Data.NHibernate.Repositories;
using NexCart.Data.NHibernate.ConfigurationRepository;
using System.Linq;
using NexCart.Business.Exceptions;
using NexCart.Business.Exceptions.ExceptionMassages;
using NexCart.Data.NHibernate.Repositories;
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
        void ChangeOrderStatus(Guid orderId, OrderStatus orderStatus);
        ResponseListModel<OrderGridViewModel> GetUserNotActiveOrders(Guid userId);
        OrderViewModelWithProducts GetUserActiveOrder(Guid userId);
        void CancelActiveOrder(Guid orderId);
        List<OrderViewModelWithProducts> GetUserOtherOrders(Guid userId);
        void MakeAnOrder(Guid shoppingCartId,Guid userId);
        void MakeAPay(OrderViewModel order);
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
                    User =user,
                    //ShoppingCart = model.ShoppingCart,
                    OrderStatus =model.OrderStatus,
                    PaymentType = model.PaymentType,

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
                                                    x.ShoppingCart.Name.Contains(request.SearchString));
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
                            ordersListForFiltering = ordersListForFiltering.OrderBy(x => x.ShoppingCart.ShoppingCartDetails);
                        }
                        else
                        {
                            ordersListForFiltering = ordersListForFiltering.OrderByDescending(x => x.ShoppingCart.ShoppingCartDetails);
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
                            ordersListForFiltering = ordersListForFiltering.OrderBy(x => x.OrderStatus);
                        }
                        else
                        {
                            ordersListForFiltering = ordersListForFiltering.OrderByDescending(x => x.OrderStatus);
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
                response.TotalItems = ordersListForFiltering.ToList().Count();

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
                entityOrder.OrderStatus = model.OrderStatus;
                
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

                var userOrder = _orderRepository.FilterBy(x => x.User == user && x.OrderStatus != OrderStatus.OnHold)
                    ?? throw new BllException(ShopExceptionMassages.OrderExceptionMassages.NOT_FOUND_EXCEPTION);

                return new ResponseListModel<OrderGridViewModel>
                {
                    Items = userOrder.Select(x => x.MapToGridViewModel()).ToList(),
                    TotalItems = userOrder.Count()
                };
            }
        }

        public OrderViewModelWithProducts GetUserActiveOrder(Guid userId)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var userOrder = _orderRepository.FilterBy(x => x.User.Id == userId && x.OrderStatus == OrderStatus.OnHold).FirstOrDefault()
                    ?? throw new BllException(ShopExceptionMassages.OrderExceptionMassages.NOT_FOUND_EXCEPTION);

                userOrder.TotalAmount = userOrder.ShoppingCart.ShoppingCartDetails.Select(x=>x.Product.Price * x.Quantity).Sum();
                _orderRepository.Update(userOrder);

                _unitOfWork.Commit();

                return userOrder.MapToViewModelWithProducts();
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

        public void MakeAnOrder(Guid shoppingCartId,Guid userId)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var shoppingCart = _shoppingCartRepository.FindBy(shoppingCartId)
                    ?? throw new BllException(ShopExceptionMassages.ShoppingCartExceptionMassages.DETAIL_NOT_FOUND_EXCEPTION);

                var user = _userRepository.FindBy(userId)
                    ?? throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);

                if(_orderRepository.CheckExist(x=>x.User == user && x.ShoppingCart == shoppingCart))
                {
                    var order = _orderRepository.FindBy(x => x.User == user && x.ShoppingCart == shoppingCart);
                    if(order == null)
                    {
                        throw new BllException(ShopExceptionMassages.OrderExceptionMassages.NOT_FOUND_EXCEPTION);
                    }

                    order.ShoppingCart = shoppingCart;
                    order.User = user;
                    order.DateCreated = DateTime.Now;
                    order.TotalAmount = shoppingCart.ShoppingCartDetails.Select(x => x.Product.Price * x.Quantity).Sum();
                    order.OrderStatus = OrderStatus.OnHold;
                    order.PaymentType = PaymentType.Cash;

                    _orderRepository.Update(order);
                    _unitOfWork.Commit();
                }
                else
                {
                    string number = Guid.NewGuid().ToString("N").Substring(0, 8);
                    Order order = new Order
                    {
                        ShoppingCart = shoppingCart,
                        User = user,
                        PaymentType = PaymentType.Cash,
                        DateCreated = DateTime.Now,
                        TotalAmount = shoppingCart.ShoppingCartDetails.Select(x=>x.Product.Price * x.Quantity).Sum(),
                        OrderName = "Order#" + number,
                        OrderStatus = OrderStatus.OnHold
                    };
                    _orderRepository.Add(order);
                    _unitOfWork.Commit();
                }
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

        public void MakeAPay(OrderViewModel model)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var order = _orderRepository.FindBy(model.Id);
                if(order == null)
                {
                    throw new BllException(ShopExceptionMassages.OrderExceptionMassages.NOT_FOUND_EXCEPTION);
                }

                var shoppingCart = _shoppingCartRepository.FindBy(model.ShoppingCart.Id);
                if (order == null)
                {
                    throw new BllException(ShopExceptionMassages.ShoppingCartExceptionMassages.NOT_FOUNG);
                }

                var user = _userRepository.FindBy(shoppingCart.User.Id);
                if (user == null)
                {
                    throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);
                }

                if (model.PaymentType == PaymentType.Cash)
                {
                    order.PaymentType = PaymentType.Cash;
                    order.OrderStatus = OrderStatus.PendingPayment;

                    shoppingCart.Active = false;

                    var newShoppingCartForUser = new ShoppingCart
                    {
                        User = user,
                        Active = true,
                        Name = "Cart#" + Guid.NewGuid().ToString("N").Substring(0, 8)
                    };

                    _shoppingCartRepository.Add(newShoppingCartForUser);
                    _shoppingCartRepository.Update(shoppingCart);

                    _orderRepository.Update(order);
                    _unitOfWork.Commit();
                }
                else if(model.PaymentType == PaymentType.Card)
                {
                    if(order.TotalAmount > user.Amount)
                    {
                        order.OrderStatus = OrderStatus.PendingPayment;

                        _orderRepository.Update(order);
                        _unitOfWork.Commit();
                    }
                    else
                    {
                        user.Amount = user.Amount - order.TotalAmount;

                        order.OrderStatus = OrderStatus.Completed;
                        order.PaymentType = PaymentType.Card; 
                    
                        shoppingCart.Active = false;

                        var newShoppingCartForUser = new ShoppingCart
                        {
                            User = user,
                            Active = true,
                            Name = "Cart#" + Guid.NewGuid().ToString("N").Substring(0, 8)
                        };

                        _shoppingCartRepository.Add(newShoppingCartForUser);
                        _shoppingCartRepository.Update(shoppingCart);

                        _orderRepository.Update(order);

                        _userRepository.Update(user);
                        _unitOfWork.Commit();
                    }
                }
                else
                {
                    // DO NOTHING
                }
            }
        }
        #endregion
    }
}
