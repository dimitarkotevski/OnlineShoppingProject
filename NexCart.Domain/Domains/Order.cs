using NexCart.Domain.Enum;
using NexCart.Domain.Enums;
using System;
using System.Collections.Generic;

namespace NexCart.Domain
{
    public class Order : BaseEntity
    {
        private User _user;
        private string _orderName;
        private ShoppingCart _shoppingCart;
        private PaymentType _paymentType;
        private OrderStatus _orderStatus;
        private decimal _totalAmount;
        private DateTime _dateCreated;
        private IList<OrderItem> _orderItems;

        public Order()
        {
            _orderItems = new List<OrderItem>();
        }

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
        public virtual decimal TotalAmount
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

        /// <summary>Immutable checkout snapshot. See <see cref="OrderItem"/>.</summary>
        public virtual IList<OrderItem> OrderItems
        {
            get => _orderItems;
            set => _orderItems = value;
        }
    }
}
