using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NexCart.Business.Services;
using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Data.NHibernate.Repositories;
using NexCart.Domain;
using NexCart.Domain.Enums;
using NSubstitute;
using Xunit;

namespace NexCart.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly IOrderRepository _orderRepo = Substitute.For<IOrderRepository>();
        private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
        private readonly IProductRepository _productRepo = Substitute.For<IProductRepository>();
        private readonly IShoppingCartRepository _cartRepo = Substitute.For<IShoppingCartRepository>();

        private OrderService CreateSut()
        {
            return new OrderService(_orderRepo, _userRepo, _productRepo, _unitOfWork, _cartRepo);
        }

        /// <summary>
        /// Makes the mocked repository apply the predicate, the same way the real
        /// repository does, so ownership filters are actually exercised.
        /// </summary>
        private void SetupOrders(params Order[] orders)
        {
            _orderRepo
                .FilterBy(Arg.Any<Expression<Func<Order, bool>>>())
                .Returns(callInfo => orders
                    .AsQueryable()
                    .Where(callInfo.Arg<Expression<Func<Order, bool>>>()));
        }

        [Fact]
        public void MakeAnOrder_copies_cart_lines_into_an_immutable_order_snapshot()
        {
            var userId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var user = new User { Id = userId, Username = "customer" };
            var product = new Product { Id = Guid.NewGuid(), Name = "Original name", Price = 10.50m };
            var cart = new ShoppingCart
            {
                Id = cartId,
                Active = true,
                User = user,
                ShoppingCartDetails = new List<ShoppingCartDetail>
                {
                    new ShoppingCartDetail
                    {
                        Product = product,
                        Quantity = 2,
                        ShoppingCart = null
                    }
                }
            };

            _cartRepo.FindBy(cartId).Returns(cart);
            _userRepo.FindBy(userId).Returns(user);
            _orderRepo.FindBy(Arg.Any<Expression<Func<Order, bool>>>()).Returns((Order)null);

            Order capturedOrder = null;
            _orderRepo.Add(Arg.Do<Order>(order => capturedOrder = order)).Returns(true);

            var sut = CreateSut();
            sut.MakeAnOrder(cartId, userId);

            Assert.NotNull(capturedOrder);
            Assert.Equal(OrderStatus.OnHold, capturedOrder.OrderStatus);
            Assert.Equal(21.00m, capturedOrder.TotalAmount);
            Assert.Same(user, capturedOrder.User);
            Assert.Same(cart, capturedOrder.ShoppingCart);

            var item = Assert.Single(capturedOrder.OrderItems);
            Assert.Equal("Original name", item.ProductName);
            Assert.Equal(10.50m, item.UnitPrice);
            Assert.Equal(2, item.Quantity);

            // Later catalogue edits must not rewrite the order snapshot.
            product.Name = "Renamed later";
            product.Price = 99.99m;

            Assert.Equal("Original name", item.ProductName);
            Assert.Equal(10.50m, item.UnitPrice);
            Assert.Equal(21.00m, capturedOrder.TotalAmount);
        }

        [Fact]
        public void GetUserOrder_returns_the_owners_snapshot()
        {
            var ownerId = Guid.NewGuid();
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderName = "Order#12345678",
                User = new User { Id = ownerId },
                OrderStatus = OrderStatus.Completed,
                TotalAmount = 21.00m,
                DateCreated = new DateTime(2026, 10, 26, 12, 0, 0, DateTimeKind.Utc),
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        Product = new Product
                        {
                            Id = Guid.NewGuid(),
                            Name = "Renamed later",
                            Price = 99.99m
                        },
                        ProductName = "Name at checkout",
                        UnitPrice = 10.50m,
                        Quantity = 2
                    }
                }
            };

            SetupOrders(order);

            var result = CreateSut().GetUserOrder(order.Id, ownerId);
            Assert.NotNull(result);
            Assert.Equal(order.Id, result.Id);
            Assert.Equal(order.DateCreated, result.DateCreated);

            var line = Assert.Single(result.Products);
            Assert.Equal("Name at checkout", line.Product.Name);
            Assert.Equal(10.50m, line.Product.Price);
            Assert.Equal(2, line.Quantity);
        }

        [Fact]
        public void GetUserOrder_only_returns_the_requested_order()
        {
            var ownerId = Guid.NewGuid();
            var otherOrder = new Order
            {
                Id = Guid.NewGuid(),
                User = new User { Id = ownerId },
                OrderItems = new List<OrderItem>()
            };
            var requestedOrder = new Order
            {
                Id = Guid.NewGuid(),
                User = new User { Id = ownerId },
                OrderItems = new List<OrderItem>()
            };

            // The other order is first, so ignoring the order id would return the wrong one.
            SetupOrders(otherOrder, requestedOrder);

            var result = CreateSut().GetUserOrder(requestedOrder.Id, ownerId);

            Assert.NotNull(result);
            Assert.Equal(requestedOrder.Id, result.Id);
        }

        [Fact]
        public void GetUserOrder_does_not_expose_another_users_order()
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                User = new User { Id = Guid.NewGuid() },
                OrderItems = new List<OrderItem>()
            };

            SetupOrders(order);

            var result = CreateSut().GetUserOrder(order.Id, Guid.NewGuid());

            Assert.Null(result);
        }
    }
}
