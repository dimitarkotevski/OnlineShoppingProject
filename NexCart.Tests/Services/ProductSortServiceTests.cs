using System;
using System.Collections.Generic;
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
        private readonly IOrderItemRepository _orderItemRepo = Substitute.For<IOrderItemRepository>();
        private readonly IFavouriteProductsRepository _favouriteRepo = Substitute.For<IFavouriteProductsRepository>();

        // Created dates make "newest first" the tie-break order: New, Mid, Old.
        private readonly Product _old = NewProduct("Old", 2020, price: 30);
        private readonly Product _mid = NewProduct("Mid", 2022, price: 10);
        private readonly Product _new = NewProduct("New", 2024, price: 20);

        public ProductSortServiceTests()
        {
            _favouriteRepo.All().Returns(Enumerable.Empty<FavouriteProducts>().AsQueryable());
            _orderItemRepo.All().Returns(Enumerable.Empty<OrderItem>().AsQueryable());
        }

        private static Product NewProduct(string name, int year, decimal price)
        {
            return new Product { Id = Guid.NewGuid(), Name = name, Price = price, Created = new DateTime(year, 1, 1) };
        }

        private string[] SortedNames(string sortName)
        {
            var sut = new ProductSortService(_orderItemRepo, _favouriteRepo);
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
        public void BestSelling_sums_quantities_from_order_items_that_were_not_canceled()
        {
            var completedOrder = new Order
            {
                OrderStatus = OrderStatus.Completed,
                OrderItems = new List<OrderItem>()
            };
            var canceledOrder = new Order
            {
                OrderStatus = OrderStatus.Canceled,
                OrderItems = new List<OrderItem>()
            };

            _orderItemRepo.All().Returns(new[]
            {
                new OrderItem { Order = completedOrder, Product = _old, Quantity = 1 },
                new OrderItem { Order = completedOrder, Product = _mid, Quantity = 3 },
                new OrderItem { Order = completedOrder, Product = _new, Quantity = 10 },
                // Cancelled orders do not count as sales. If this line were included,
                // Old would score 101 and sort first.
                new OrderItem { Order = canceledOrder, Product = _old, Quantity = 100 },
            }.AsQueryable());

            Assert.Equal(new[] { "New", "Mid", "Old" }, SortedNames(ProductSortNames.BestSelling));
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
