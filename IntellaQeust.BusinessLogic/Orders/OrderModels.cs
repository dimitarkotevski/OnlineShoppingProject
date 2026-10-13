using FluentValidation;
using IntellaQuest.Domain;
using IntellaQuest.Domain.Enum;
using IntellaQuest.Domain.Enums;
using System;
using System.Collections.Generic;

namespace IntellaQeust.BusinessLogic.ViewModels
{
    //[Validator(typeof(OrderViewModelValidator))]
    public class OrderViewModel
    {
        public Guid Id { get; set; }
        public string OrderName { get; set; }
        public PaymentType PaymentType { get; set; }
        public LookupViewModel ShoppingCart { get; set; }
        public LookupViewModel User { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public double TotalAmount { get; set; }
        public DateTime DateCreated { get; set; }
    }
    public class OrderViewModelWithProducts
    {
        public Guid Id { get; set; }
        public string OrderName { get; set; }
        public List<ShoppingCartDetailViewModel> Products { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentType PaymentType { get; set; }
        public double TotalAmount { get; set; }
        public LookupViewModel ShoppingCart { get; set; }
    }
    public class OrderGridViewModel
    {
        public Guid Id { get; set; }
        public string OrderName { get; set; }
        public int NumberOfProducts { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public double TotalAmount { get; set; }
        public DateTime DateCreated { get; set; }
        public PaymentType PaymentType { get; set; }
    }
    public class OrderViewModelValidator : AbstractValidator<Order>
    {
        public OrderViewModelValidator()
        {
            /*RuleFor(x => x.User).NotEmpty().WithMessage("User is required.");
            RuleFor(x => x.ShoppingCart).NotEmpty().WithMessage("Shopping Card is required.");
            RuleFor(x => x.OrderStatus).NotEmpty().WithMessage("Order Status is required.");*/
        }
    }
}
