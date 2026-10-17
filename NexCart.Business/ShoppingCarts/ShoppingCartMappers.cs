using NexCart.Business.ViewModels;
using NexCart.Business.Mappers;
using NexCart.Domain;
using NHibernate.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace NexCart.Business.Mappers
{
    public static class ShoppingCartMappers
    {
        public static ShoppingCartViewModel MapToViewModel(this ShoppingCart model)
        {
            return new ShoppingCartViewModel
            {
                Id = model.Id,
                Name = model.Name,
                ShoppingCartDetails = model.ShoppingCartDetails.Select(x => x.MapToViewModel()).ToList()
            };
        }
    }
}
