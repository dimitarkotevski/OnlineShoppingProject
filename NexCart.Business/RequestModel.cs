using NHibernate.Mapping;
using System;
using System.Collections.Generic;
using FluentValidation;
using NexCart.Business.Validation;
using NexCart.Domain.Enum;
using NexCart.Domain.Enums;

namespace NexCart.Business.Requests
{
    [FluentValidator(typeof(RequestModelValidator))]
    public class RequestModel
    {
        public string SearchString { get; set; }
        public string SortName { get; set; }
        public string isAscending { get; set; }
        public int PageNeeded { get; set; }
        public int Size { get; set; }
        public Guid? EntityId { get; set; }
        public List<string> SearchBrands { get; set; }
        public List<string> SearchList { get; set; }
        /// <summary>Lower bound for product price or order total (inclusive).</summary>
        public double? PriceFrom { get; set; }
        /// <summary>Upper bound for product price or order total (inclusive).</summary>
        public double? PriceTo { get; set; }
        public string SearchStatus { get; set; }
        /// <summary>Users whose email ends with this text, e.g. "@gmail.com".</summary>
        public string EmailEnding { get; set; }

        // Admin grid filters. Every filter is optional; null means "any".
        /// <summary>Product created / order placed on or after this day.</summary>
        public DateTime? DateFrom { get; set; }
        /// <summary>Product created / order placed on or before this day (the whole day counts).</summary>
        public DateTime? DateTo { get; set; }
        /// <summary>true: only products with an image; false: only products without one.</summary>
        public bool? HasImage { get; set; }
        public OrderStatus? OrderStatus { get; set; }
        public PaymentType? PaymentType { get; set; }
        public UserRole? Role { get; set; }
        /// <summary>Categories: true for active only, false for inactive only.</summary>
        public bool? IsActive { get; set; }
        public string City { get; set; }
    }

    /// <summary>
    /// Rules for grid paging and filters. Every filter stays optional; only values that are
    /// present are checked, and ranges only when both ends are given.
    /// </summary>
    public class RequestModelValidator : AbstractValidator<RequestModel>
    {
        public const int MaxPageSize = 100;
        public const int MaxTextLength = 100;

        public RequestModelValidator()
        {
            RuleFor(x => x.PageNeeded).GreaterThanOrEqualTo(0).WithMessage("Page must not be negative.");
            // Size 0 means "everything" (storefront lists); grids page at most MaxPageSize rows.
            RuleFor(x => x.Size).InclusiveBetween(0, MaxPageSize)
                .WithMessage($"Page size must be between 1 and {MaxPageSize}.");

            RuleFor(x => x.SearchString).MaximumLength(MaxTextLength)
                .WithMessage($"Search must be at most {MaxTextLength} characters.");
            RuleFor(x => x.City).MaximumLength(MaxTextLength)
                .WithMessage($"City must be at most {MaxTextLength} characters.");
            RuleFor(x => x.EmailEnding).MaximumLength(MaxTextLength)
                .WithMessage($"Email domain must be at most {MaxTextLength} characters.");

            RuleFor(x => x.PriceFrom).GreaterThanOrEqualTo(0).When(x => x.PriceFrom.HasValue)
                .WithMessage("Minimum must not be negative.");
            RuleFor(x => x.PriceTo).GreaterThanOrEqualTo(0).When(x => x.PriceTo.HasValue)
                .WithMessage("Maximum must not be negative.");
            RuleFor(x => x.PriceTo).GreaterThanOrEqualTo(x => x.PriceFrom.Value)
                .When(x => x.PriceFrom.HasValue && x.PriceTo.HasValue)
                .WithMessage("Maximum must be greater than or equal to the minimum.");

            RuleFor(x => x.DateTo).GreaterThanOrEqualTo(x => x.DateFrom.Value)
                .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
                .WithMessage("'To' date must be on or after the 'from' date.");

            RuleFor(x => x.OrderStatus).IsInEnum().WithMessage("Unknown order status.");
            RuleFor(x => x.PaymentType).IsInEnum().WithMessage("Unknown payment type.");
            RuleFor(x => x.Role).IsInEnum().WithMessage("Unknown role.");
        }
    }
}
