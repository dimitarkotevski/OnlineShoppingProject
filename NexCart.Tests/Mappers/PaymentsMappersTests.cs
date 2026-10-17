using System;
using NexCart.Business.Mappers;
using NexCart.Domain;
using Xunit;

namespace NexCart.Tests.Mappers
{
    public class PaymentsMappersTests
    {
        [Fact]
        public void MapToViewModel_maps_properties_without_security_code()
        {
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                CardNumber = "1234",
                ExpirationDate = new DateTime(2029, 6, 1)
            };

            var vm = payment.MapToViewModel();

            Assert.Equal(payment.Id, vm.Id);
            Assert.Equal("1234", vm.CardNumber);
            Assert.Equal(new DateTime(2029, 6, 1), vm.ExpirationDate);
            Assert.Null(vm.SecurityCode);
        }

        [Fact]
        public void MapToViewModelInfo_maps_address_fields()
        {
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                CardNumber = "9999",
                ExpirationDate = new DateTime(2030, 1, 1),
                City = "Seattle",
                BillingAdress = "456 Pine St",
                ZipPostalCode = "98101",
                Country = "USA",
                PhoneNumber = "555-0199"
            };

            var vmInfo = payment.MapToViewModelInfo();

            Assert.Equal(payment.Id, vmInfo.Id);
            Assert.Equal("9999", vmInfo.CardNumber);
            Assert.Equal("Seattle", vmInfo.City);
            Assert.Equal("456 Pine St", vmInfo.BillingAddress);
            Assert.Equal("98101", vmInfo.ZipPostalCode);
            Assert.Equal("USA", vmInfo.Country);
            Assert.Equal("555-0199", vmInfo.PhoneNumber);
            Assert.Null(vmInfo.SecurityCode);
        }

        [Fact]
        public void MapToViewModelCard_maps_correctly()
        {
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                CardNumber = "8888",
                ExpirationDate = new DateTime(2031, 3, 1)
            };

            var vmCard = payment.MapToViewModelCard();

            Assert.Equal(payment.Id, vmCard.Id);
            Assert.Equal("8888", vmCard.CardNumber);
            Assert.Equal(payment.ExpirationDate, vmCard.ExpirationDate);
            Assert.Null(vmCard.SecurityCode);
        }
    }
}
