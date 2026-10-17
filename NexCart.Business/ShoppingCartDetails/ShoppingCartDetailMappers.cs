using NexCart.Business.ViewModels;
using NexCart.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexCart.Business.Mappers
{
    public static class ShoppingCartDetailMappers
    {
        public static ShoppingCartDetailViewModel MapToViewModel(this ShoppingCartDetail model)
        {
            return new ShoppingCartDetailViewModel
            {
                Id = model.Id,
                Product = model.Product.MapToViewModel(),
                Quantity = model.Quantity
            };
        }
    }
}
