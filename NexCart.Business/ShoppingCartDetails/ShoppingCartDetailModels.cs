using NHibernate.Mapping;
using System;
using System.Collections.Generic;

namespace NexCart.Business.ViewModels
{
    public class ShoppingCartDetailViewModel
    {
        public Guid Id { get; set; }
        public ProductViewModel Product { get; set; }
        public int Quantity { get; set; }
    }
}
