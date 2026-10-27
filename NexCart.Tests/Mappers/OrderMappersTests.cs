using System;
using System.Collections.Generic;
using NexCart.Business.Mappers;
using NexCart.Domain;
using NexCart.Domain.Enums;
using Xunit;

namespace NexCart.Tests.Mappers
{
    public class OrderMappersTests
    {
        [Fact]
        public void Order_details_use_the_snapshot_instead_of_current_product_values()
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderName = "Order#12345678",
                OrderStatus = OrderStatus.Completed,
                TotalAmount = 42.00m,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        Product = new Product
                        {
                            Id = Guid.NewGuid(),
                            Name = "Renamed in catalogue",
                            Price = 100.00m
                        },
                        ProductName = "Name at checkout",
                        UnitPrice = 21.00m,
                        Quantity = 2
                    }
                }
            };

            var viewModel = order.MapToViewModelWithProducts();

            Assert.Equal(42.00m, viewModel.TotalAmount);
            var product = Assert.Single(viewModel.Products);
            Assert.Equal("Name at checkout", product.Product.Name);
            Assert.Equal(21.00m, product.Product.Price);
            Assert.Equal(2, product.Quantity);
        }
    }
}
