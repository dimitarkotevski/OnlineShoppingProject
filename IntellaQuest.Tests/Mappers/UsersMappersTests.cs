using System;
using System.Collections.Generic;
using IntellaQuest.BusinessLogic.Mappers;
using IntellaQuest.Domain;
using Xunit;

namespace IntellaQuest.Tests.Mappers
{
    public class UsersMappersTests
    {
        [Fact]
        public void MapToViewModel_maps_basic_properties()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "jdoe",
                FirstName = "John",
                LastName = "Doe",
                Email = "jdoe@example.com",
                Address = "123 Main St",
                Payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    CardNumber = "4321",
                    ExpirationDate = new DateTime(2027, 5, 1)
                }
            };

            var vm = user.MapToViewModel();

            Assert.Equal(user.Id, vm.Id);
            Assert.Equal("jdoe", vm.Username);
            Assert.Equal("John", vm.FirstName);
            Assert.Equal("Doe", vm.LastName);
            Assert.Equal("jdoe@example.com", vm.Email);
            Assert.Equal("123 Main St", vm.Address);
            Assert.NotNull(vm.Payment);
            Assert.Equal("4321", vm.Payment.CardNumber);
        }

        [Fact]
        public void MapToGridModel_maps_all_grid_fields()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "jdoe",
                FirstName = "John",
                LastName = "Doe",
                Email = "jdoe@example.com",
                Address = "123 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "10001",
                Role = new Role { Name = "Customer" }
            };

            var grid = user.MapToGridModel();

            Assert.Equal(user.Id, grid.Id);
            Assert.Equal("jdoe", grid.Username);
            Assert.Equal("John", grid.FirstName);
            Assert.Equal("Doe", grid.LastName);
            Assert.Equal("Customer", grid.Role);
            Assert.Equal("New York", grid.City);
            Assert.Equal("NY", grid.State);
            Assert.Equal("10001", grid.ZipCode);
        }

        [Theory]
        [InlineData("1234567812345678", "**** **** **** 5678")]
        [InlineData("5678", "**** **** **** 5678")]
        [InlineData("123", "**** **** **** 123")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void MapToUserDetailModel_masks_card_number_correctly(string inputCard, string expectedMasked)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "jdoe",
                FirstName = "John",
                LastName = "Doe",
                Email = "jdoe@example.com",
                Payment = inputCard != null || expectedMasked != "" ? new Payment
                {
                    Id = Guid.NewGuid(),
                    CardHolder = "John Doe",
                    CardNumber = inputCard,
                    ExpirationDate = new DateTime(2028, 12, 1)
                } : null,
                FavouriteProducts = new List<Product>()
            };

            var details = user.MapToUserDetailModel();

            if (user.Payment == null)
            {
                Assert.Null(details.Payment);
            }
            else
            {
                Assert.NotNull(details.Payment);
                Assert.Equal(expectedMasked, details.Payment.CardNumber);
                Assert.Equal("12/28", details.Payment.ExpirationDate);
            }
        }

        [Fact]
        public void MapToLookupViewModel_combines_first_and_last_name()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Austin"
            };

            var lookup = user.MapToLookupViewModel();

            Assert.Equal(user.Id, lookup.Id);
            Assert.Equal("Jane Austin", lookup.Name);
            Assert.True(lookup.Status);
        }

        [Fact]
        public void MapToLookupUsernameViewModel_uses_username()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "janeaustin"
            };

            var lookup = user.MapToLookupUsernameViewModel();

            Assert.Equal(user.Id, lookup.Id);
            Assert.Equal("janeaustin", lookup.Name);
            Assert.True(lookup.Status);
        }
    }
}
