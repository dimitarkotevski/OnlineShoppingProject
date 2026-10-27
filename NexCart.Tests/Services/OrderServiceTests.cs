using System;
using System.Collections.Generic;
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
    }
}
