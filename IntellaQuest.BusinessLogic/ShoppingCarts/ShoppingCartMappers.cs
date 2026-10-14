using IntellaQuest.BusinessLogic.ViewModels;
using IntellaQuest.BusinessLogic.Mappers;
using IntellaQuest.Domain;
using NHibernate.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace IntellaQuest.BusinessLogic.Mappers
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
