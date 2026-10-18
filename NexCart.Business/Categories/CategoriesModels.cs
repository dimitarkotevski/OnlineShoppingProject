using System;
using FluentValidation;
using NexCart.Business.Validation;

namespace NexCart.Business.ViewModels
{
    public class CategoryLookupViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }
        public string Url { get; set; }
    }

    /// <summary>Row of the admin categories grid.</summary>
    public class CategoryGridModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        /// <summary>Active categories are listed in the storefront navbar.</summary>
        public bool Status { get; set; }
        public string Url { get; set; }
        public int ProductCount { get; set; }
    }

    /// <summary>Admin create / edit payload. Id is empty when creating.</summary>
    [FluentValidator(typeof(CategoryViewModelValidator))]
    public class CategoryViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; } = true;
    }

    public class CategoryViewModelValidator : AbstractValidator<CategoryViewModel>
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 50;

        public CategoryViewModelValidator()
        {
            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Category name is required.")
                .Must(name => name.Trim().Length >= NameMinLength)
                    .WithMessage($"Category name must be at least {NameMinLength} characters.")
                .Must(name => name.Trim().Length <= NameMaxLength)
                    .WithMessage($"Category name must be at most {NameMaxLength} characters.")
                // The name becomes the URL slug, so it needs at least one letter or digit.
                .Must(HasLetterOrDigit)
                    .WithMessage("Category name must contain a letter or digit.");
        }

        private static bool HasLetterOrDigit(string name)
        {
            foreach (var c in name)
            {
                if (char.IsLetterOrDigit(c)) return true;
            }
            return false;
        }
    }
}
