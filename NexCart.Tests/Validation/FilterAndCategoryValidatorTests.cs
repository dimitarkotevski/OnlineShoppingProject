using System;
using FluentValidation.TestHelper;
using NexCart.Business.Requests;
using NexCart.Business.ViewModels;
using NexCart.Domain.Enums;
using Xunit;

namespace NexCart.Tests.Validation
{
    public class FilterAndCategoryValidatorTests
    {
        private readonly RequestModelValidator _requestValidator = new();
        private readonly CategoryViewModelValidator _categoryValidator = new();

        [Fact]
        public void Request_with_no_filters_is_valid()
        {
            _requestValidator.TestValidate(new RequestModel()).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Request_with_valid_filters_is_valid()
        {
            var request = new RequestModel
            {
                PageNeeded = 2,
                Size = 10,
                PriceFrom = 10,
                PriceTo = 10,
                DateFrom = new DateTime(2026, 1, 1),
                DateTo = new DateTime(2026, 1, 1),
                OrderStatus = OrderStatus.Completed,
                Role = UserRole.Admin,
            };

            _requestValidator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Negative_prices_are_rejected()
        {
            var result = _requestValidator.TestValidate(new RequestModel { PriceFrom = -1, PriceTo = -5 });

            result.ShouldHaveValidationErrorFor(x => x.PriceFrom);
            result.ShouldHaveValidationErrorFor(x => x.PriceTo);
        }

        [Fact]
        public void Maximum_below_minimum_is_rejected()
        {
            _requestValidator.TestValidate(new RequestModel { PriceFrom = 100, PriceTo = 50 })
                .ShouldHaveValidationErrorFor(x => x.PriceTo);
        }

        [Fact]
        public void Date_range_ending_before_it_starts_is_rejected()
        {
            var request = new RequestModel { DateFrom = new DateTime(2026, 5, 2), DateTo = new DateTime(2026, 5, 1) };

            _requestValidator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.DateTo);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(RequestModelValidator.MaxPageSize + 1)]
        public void Page_size_outside_range_is_rejected(int size)
        {
            _requestValidator.TestValidate(new RequestModel { Size = size })
                .ShouldHaveValidationErrorFor(x => x.Size);
        }

        [Fact]
        public void Unknown_enum_values_are_rejected()
        {
            var result = _requestValidator.TestValidate(new RequestModel
            {
                OrderStatus = (OrderStatus)42,
                Role = (UserRole)42,
            });

            result.ShouldHaveValidationErrorFor(x => x.OrderStatus);
            result.ShouldHaveValidationErrorFor(x => x.Role);
        }

        [Fact]
        public void Too_long_text_filters_are_rejected()
        {
            var tooLong = new string('a', RequestModelValidator.MaxTextLength + 1);
            var result = _requestValidator.TestValidate(new RequestModel { SearchString = tooLong, City = tooLong });

            result.ShouldHaveValidationErrorFor(x => x.SearchString);
            result.ShouldHaveValidationErrorFor(x => x.City);
        }

        [Theory]
        [InlineData("Laptops")]
        [InlineData("Headphones & Audio")]
        [InlineData("TV")]
        public void Valid_category_names_pass(string name)
        {
            _categoryValidator.TestValidate(new CategoryViewModel { Name = name }).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("A")]
        [InlineData("&&&")]
        public void Invalid_category_names_fail(string name)
        {
            _categoryValidator.TestValidate(new CategoryViewModel { Name = name })
                .ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Category_name_longer_than_the_limit_fails()
        {
            var name = new string('a', CategoryViewModelValidator.NameMaxLength + 1);

            _categoryValidator.TestValidate(new CategoryViewModel { Name = name })
                .ShouldHaveValidationErrorFor(x => x.Name);
        }
    }
}
