using FluentValidation;
using System;
using System.Collections.Generic;

namespace NexCart.Business.ViewModels
{
/*    [Validator(typeof(ProductsViewModelValidator))]*/    
    public class ProductViewModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string ImageUrl { get; set; }
        public string Image { get; set; }
        public DateTime DateCreated { get; set; }
        public CategoryLookupViewModel Category { get; set; }
    }
    public class ProductsViewModelValidator : AbstractValidator<ProductViewModel>
    {
        public ProductsViewModelValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        }
    }
}
