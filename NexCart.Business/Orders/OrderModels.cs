using NexCart.Domain.Enum;
using NexCart.Domain.Enums;
using System;
using System.Collections.Generic;

namespace NexCart.Business.ViewModels
{
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
}
