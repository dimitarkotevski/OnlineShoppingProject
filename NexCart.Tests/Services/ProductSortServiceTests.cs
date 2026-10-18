using System;
using System.Linq;
using NexCart.Business.Services;
using NexCart.Data.NHibernate.Repositories;
using NexCart.Domain;
using NexCart.Domain.Enums;
using NSubstitute;
using Xunit;

namespace NexCart.Tests.Services
{
    public class ProductSortServiceTests
    {
        private readonly IOrderRepository _orderRepo = Substitute.For<IOrderRepository>();
        private readonly IFavouriteProductsRepository _favouriteRepo = Substitute.For<IFavouriteProductsRepository>();
        private readonly IShoppingCartDetailRepository _detailRepo = Substitute.For<IShoppingCartDetailRepository>();

        // Created dates make "newest first" the tie-break order: New, Mid, Old.
        private readonly Product _old = NewProduct("Old", 2020, price: 30);
        private readonly Product _mid = NewProduct("Mid", 2022, price: 10);
        private readonly Product _new = NewProduct("New", 2024, price: 20);

        public ProductSortServiceTests()
        {
            _favouriteRepo.All().Returns(Enumerable.Empty<FavouriteProducts>().AsQueryable());
            _orderRepo.All().Returns(Enumerable.Empty<Order>().AsQueryable());
            _detailRepo.All().Returns(Enumerable.Empty<ShoppingCartDetail>().AsQueryable());
        }

        private static Product NewProduct(string name, int year, double price)
        {
            return new Product { Id = Guid.NewGuid(), Name = name, Price = price, Created = new DateTime(year, 1, 1) };
        }

        private string[] SortedNames(string sortName)
        {
            var sut = new ProductSortService(_orderRepo, _favouriteRepo, _detailRepo);
            return sut.Sort(new[] { _old, _mid, _new }, sortName).Select(p => p.Name).ToArray();
        }

        [Fact]
        public void Popular_orders_by_favourite_count_then_newest()
        {
            _favouriteRepo.All().Returns(new[]
            {
                new FavouriteProducts { Product = _old },
                new FavouriteProducts { Product = _old },
                new FavouriteProducts { Product = _mid },
            }.AsQueryable());

            Assert.Equal(new[] { "Old", "Mid", "New" }, SortedNames(ProductSortNames.Popular));
        }

        [Fact]
        public void Popular_with_no_favourites_falls_back_to_newest()
        {
            Assert.Equal(new[] { "New", "Mid", "Old" }, SortedNames(ProductSortNames.Popular));
        }

        [Fact]
        public void BestSelling_sums_quantities_from_orders_that_were_not_canceled()
        {
            var soldCart = new ShoppingCart { Id = Guid.NewGuid() };
            var canceledCart = new ShoppingCart { Id = Guid.NewGuid() };
            var openCart = new ShoppingCart { Id = Guid.NewGuid() };

            _orderRepo.All().Returns(new[]
            {
                new Order { ShoppingCart = soldCart, OrderStatus = OrderStatus.Completed },
                new Order { ShoppingCart = canceledCart, OrderStatus = OrderStatus.Canceled },
            }.AsQueryable());

            _detailRepo.All().Returns(new[]
            {
                new ShoppingCartDetail { ShoppingCart = soldCart, Product = _old, Quantity = 1 },
                new ShoppingCartDetail { ShoppingCart = soldCart, Product = _mid, Quantity = 3 },
                // Canceled orders and carts that were never ordered do not count as sales.
                new ShoppingCartDetail { ShoppingCart = canceledCart, Product = _new, Quantity = 10 },
                new ShoppingCartDetail { ShoppingCart = openCart, Product = _new, Quantity = 10 },
            }.AsQueryable());

            Assert.Equal(new[] { "Mid", "Old", "New" }, SortedNames(ProductSortNames.BestSelling));
        }

        [Theory]
        [InlineData(ProductSortNames.Newest, new[] { "New", "Mid", "Old" })]
        [InlineData(ProductSortNames.PriceLow, new[] { "Mid", "New", "Old" })]
        [InlineData(ProductSortNames.PriceHigh, new[] { "Old", "New", "Mid" })]
        [InlineData(null, new[] { "Old", "Mid", "New" })]
        public void Simple_sorts(string sortName, string[] expected)
        {
            Assert.Equal(expected, SortedNames(sortName));
        }
    }
}
