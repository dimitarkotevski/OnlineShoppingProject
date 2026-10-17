
using System;
using System.Collections.Generic;

namespace NexCart.Domain
{
    public class Product : BaseEntity
    {
        private string _name;
        private double _price;
        private string _imageUrl;
        private string _description;
        private string _tags;

        private Category _category;
        private IList<Order> _orders;
        private DateTime _created;
        public Product()
        {
            _orders = new List<Order>();
        }
        public virtual string Name 
        { 
            get => _name; 
            set => _name = value; 
        }
        public virtual string Tags
        {
            get => _tags;
            set => _tags = value;
        }
        public virtual string Description 
        {
            get => _description; 
            set => _description = value; 
        }
        public virtual Category Category 
        { 
            get => _category; 
            set => _category = value; 
        }
        public virtual IList<Order> Orders
        {
            get => _orders;
            set => _orders = value;
        }
        public virtual double Price
        {
            get => _price;
            set => _price = value;
        }
        public virtual string ImageUrl
        {
            get => _imageUrl;
            set => _imageUrl = value;
        }

        public virtual string Image
        {
            get => _imageUrl;
            set => _imageUrl = value;
        }
        public virtual DateTime Created
        {
            get => _created;
            set => _created = value;
        }
    }
}
