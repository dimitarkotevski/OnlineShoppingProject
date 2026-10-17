using FluentValidation;
using NexCart.Business.Validation;
using NexCart.Business.ViewModels;
using NHibernate.Mapping;
using System;
using System.Collections.Generic;

namespace NexCart.Business.ViewModels
{
    [FluentValidator(typeof(ShoppingCartsViewModelValidator))]
    public class ShoppingCartViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<ShoppingCartDetailViewModel> ShoppingCartDetails { get; set; }
    }
    public class ShoppingCartGridViewModel
    {
        public Guid Id { get; set; }
        public double Quality { get; set; }
        public ProductViewModel Product { get; set; }
    }
    public class ShoppingCartsViewModelValidator : AbstractValidator<ShoppingCartViewModel>
    {
        public ShoppingCartsViewModelValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        }
    }
}
