using NexCart.Domain.Enum;
using NexCart.Domain.Enums;
using System;

namespace NexCart.Domain
{
    public class Order : BaseEntity
    {
        private User _user;
        private string _orderName;
        private ShoppingCart _shoppingCart;
        private PaymentType _paymentType;
        private OrderStatus _orderStatus;
        private double _totalAmount;
        private DateTime _dateCreated;
        public Order() { }
        public virtual User User 
        {
            get => _user;
            set => _user = value;
        }
        public virtual ShoppingCart ShoppingCart
        {
            get => _shoppingCart;
            set => _shoppingCart = value;
        }
        public virtual PaymentType PaymentType
        {
            get => _paymentType;
            set => _paymentType = value;
        }
        public virtual OrderStatus OrderStatus
        {
            get => _orderStatus; 
            set { _orderStatus = value; }
        }
        public virtual double TotalAmount
        {
            get => _totalAmount;
            set => _totalAmount = value;
        }
        public virtual DateTime DateCreated
        {
            get => _dateCreated;
            set => _dateCreated = value;
        }
        public virtual string OrderName
        {
            get => _orderName;
            set => _orderName = value;
        }
    }
}
