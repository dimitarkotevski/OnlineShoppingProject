using System;
using System.Collections.Generic;
using NexCart.Domain.Enums;

namespace NexCart.Domain
{
    public class User : BaseEntity
    {
        private string _firstName;
        private string _lastName;
        private string _email;
        private string _username;
        private string _password;
        private string _description;
        private string _address;
        private string _state;
        private string _city;
        private string _zipCode;
        private decimal _amount;
        private Payment _payment;
        private UserRole _role;
        private IList<Order> _orders;
        private IList<Product> _favouriteProducts;
        public User() 
        {
            _orders = new List<Order>();
        }
        public virtual string FirstName
        {
            get => _firstName;
            set => _firstName = value;
        }
        public virtual string LastName
        {
            get => _lastName;
            set => _lastName = value;
        }
        public virtual string Email
        {
            get => _email;
            set => _email = value;
        }
        public virtual string Username
        {
            get => _username;
            set => _username = value;
        }
        public virtual string Password
        {
            get => _password;
            set => _password = value;
        }
        public virtual string Description
        {
            get => _description; 
            set => _description = value;
        }
        public virtual string Address
        {
            get => _address; 
            set => _address = value;
        }
        public virtual string State
        {
            get => _state;
            set => _state = value;
        }
        public virtual string City
        {
            get => _city;
            set => _city = value;
        }
        public virtual string ZipCode
        {
            get => _zipCode;
            set => _zipCode = value;
        }
        public virtual decimal Amount
        {
            get => _amount; 
            set => _amount = value;
        }
        public virtual IList<Product> FavouriteProducts
        {
            get => _favouriteProducts;
            set => _favouriteProducts = value;
        }
        public virtual IList<Order> Orders 
        { 
            get => _orders; 
            set=> _orders = value;
        }
        public virtual Payment Payment
        {
            get => _payment;
            set => _payment = value;
        }
        public virtual UserRole Role
        {
            get => _role;
            set => _role = value;
        }
    }
}
