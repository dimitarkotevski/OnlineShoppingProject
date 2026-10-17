using System;
using System.Collections.Generic;
using System.Text;
using NexCart.Business.Mappers;
using NexCart.Domain;
using Xunit;

namespace NexCart.Tests.Mappers
{
    public class EntityMappersTests
    {
        [Fact]
        public void Category_MapToViewModel_maps_fields_and_product_lookups()
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Electronics",
                Status = true,
                Url = "electronics",
                Products = new List<Product>
                {
                    new Product { Id = Guid.NewGuid(), Name = "Laptop" },
                    new Product { Id = Guid.NewGuid(), Name = "Phone" }
                }
            };

            var vm = category.MapToViewModel();

            Assert.Equal(category.Id, vm.Id);
            Assert.Equal("Electronics", vm.Name);
            Assert.True(vm.Status);
            Assert.Equal(2, vm.Products.Count);
            Assert.Equal("Laptop", vm.Products[0].Name);
        }

        [Fact]
        public void Category_MapToLookupViewModel_maps_url_and_status()
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Books",
                Status = true,
                Url = "books"
            };

            var lookup = category.MapToLookupViewModel();

            Assert.Equal(category.Id, lookup.Id);
            Assert.Equal("Books", lookup.Name);
            Assert.True(lookup.Status);
            Assert.Equal("books", lookup.Url);
        }

        [Fact]
        public void Product_MapToViewModel_maps_details_and_decodes_image()
        {
            var imageString = "sample-image.jpg";
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Headphones",
                Description = "Noise cancelling",
                Price = 199.99,
                Image = imageString,
                Created = new DateTime(2026, 1, 1),
                Category = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Audio",
                    Status = true,
                    Url = "audio"
                }
            };

            var vm = product.MapToViewModel();

            Assert.Equal(product.Id, vm.Id);
            Assert.Equal("Headphones", vm.Name);
            Assert.Equal("Noise cancelling", vm.Description);
            Assert.Equal(199.99, vm.Price);
            Assert.Equal(imageString, vm.Image);
            Assert.Equal(new DateTime(2026, 1, 1), vm.DateCreated);
            Assert.NotNull(vm.Category);
            Assert.Equal("Audio", vm.Category.Name);
        }

        [Fact]
        public void Product_MapToViewModel_handles_null_image()
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Keyboard",
                Image = null,
                Category = new Category { Name = "Accessories" }
            };

            var vm = product.MapToViewModel();

            Assert.Equal("", vm.Image);
        }
    }
}
