using System;
using NexCart.Business.Mappers;
using NexCart.Domain;
using Xunit;

namespace NexCart.Tests.Mappers
{
    public class EntityMappersTests
    {
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
                Price = 199.99m,
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
            Assert.Equal(199.99m, vm.Price);
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
