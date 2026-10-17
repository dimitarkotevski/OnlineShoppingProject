
using System.Collections.Generic;

namespace NexCart.Domain
{
    public class Category : BaseEntity
    {
        private string _name;
        private bool _status;
        private IList<Product> _products;
        private string _url;
        public Category()
        {
            _products = new List<Product>();
        }
        public virtual string Name
        {
            get => _name;
            set => _name = value;
        }
        public virtual bool Status
        {
            get => _status;
            set => _status = value;
        }
        public virtual IList<Product> Products
        {
            get => _products;
            set => _products = value;
        }
        public virtual string Url
        {
            get => _url;
            set => _url = value;
        }
    }
}
