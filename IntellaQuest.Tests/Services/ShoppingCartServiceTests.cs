using System;
using IntellaQuest.BusinessLogic.Exceptions;
using IntellaQuest.BusinessLogic.Services;
using IntellaQuest.Data.NHibernate.ConfigurationRepository;
using IntellaQuest.Data.NHibernate.Repositories;
using IntellaQuest.Domain;
using NSubstitute;
using Xunit;

namespace IntellaQuest.Tests.Services
{
    public class ShoppingCartServiceTests
    {
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly IShoppingCartRepository _cartRepo = Substitute.For<IShoppingCartRepository>();
        private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
        private readonly IProductRepository _productRepo = Substitute.For<IProductRepository>();
        private readonly IShoppingCartDetailRepository _detailRepo = Substitute.For<IShoppingCartDetailRepository>();

        private ShoppingCartService CreateSut()
        {
            return new ShoppingCartService(_cartRepo, _unitOfWork, _userRepo, _productRepo, _detailRepo);
        }

        [Fact]
        public void MinusQuantity_does_not_drop_below_one()
        {
            var sut = CreateSut();
            var detailId = Guid.NewGuid();
            var detail = new ShoppingCartDetail
            {
                Id = detailId,
                Quantity = 1
            };

            _detailRepo.FindBy(detailId).Returns(detail);

            sut.MinusQuantity(detailId);

            Assert.Equal(1, detail.Quantity);
            _detailRepo.Received(1).Update(detail);
        }

        [Fact]
        public void MinusQuantity_decrements_when_above_one()
        {
            var sut = CreateSut();
            var detailId = Guid.NewGuid();
            var detail = new ShoppingCartDetail
            {
                Id = detailId,
                Quantity = 5
            };

            _detailRepo.FindBy(detailId).Returns(detail);

            sut.MinusQuantity(detailId);

            Assert.Equal(4, detail.Quantity);
            _detailRepo.Received(1).Update(detail);
        }

        [Fact]
        public void MinusQuantity_throws_when_detail_not_found()
        {
            var sut = CreateSut();
            var detailId = Guid.NewGuid();
            _detailRepo.FindBy(detailId).Returns((ShoppingCartDetail)null);

            Assert.Throws<BllException>(() => sut.MinusQuantity(detailId));
        }

        [Fact]
        public void PlusQuantity_increments_quantity()
        {
            var sut = CreateSut();
            var detailId = Guid.NewGuid();
            var detail = new ShoppingCartDetail
            {
                Id = detailId,
                Quantity = 3
            };

            _detailRepo.FindBy(detailId).Returns(detail);

            sut.PlusQuantity(detailId);

            Assert.Equal(4, detail.Quantity);
            _detailRepo.Received(1).Update(detail);
        }

        [Fact]
        public void PlusQuantity_caps_at_max_limit()
        {
            var sut = CreateSut();
            var detailId = Guid.NewGuid();
            var detail = new ShoppingCartDetail
            {
                Id = detailId,
                Quantity = 999
            };

            _detailRepo.FindBy(detailId).Returns(detail);

            sut.PlusQuantity(detailId);

            Assert.Equal(999, detail.Quantity);
            _detailRepo.Received(1).Update(detail);
        }

        [Fact]
        public void AddProductToShoppingCart_throws_when_user_not_found()
        {
            var sut = CreateSut();
            var userId = Guid.NewGuid();
            _userRepo.FindBy(userId).Returns((User)null);

            Assert.Throws<BllException>(() => sut.AddProductToShoppingCart(userId, Guid.NewGuid(), 1));
        }

        [Fact]
        public void AddProductToShoppingCart_throws_when_product_not_found()
        {
            var sut = CreateSut();
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            _userRepo.FindBy(userId).Returns(new User { Id = userId });
            _productRepo.FindBy(productId).Returns((Product)null);

            Assert.Throws<BllException>(() => sut.AddProductToShoppingCart(userId, productId, 1));
        }
    }
}
