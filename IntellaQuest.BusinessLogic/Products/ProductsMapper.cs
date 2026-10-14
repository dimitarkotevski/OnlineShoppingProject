using IntellaQuest.BusinessLogic.ViewModels;
using IntellaQuest.BusinessLogic.Mappers;
using IntellaQuest.Domain;
using System;
using System.Linq;

namespace IntellaQuest.BusinessLogic.Mappers
{
    public static class ProductsMappers
    {
        public static ProductViewModel MapToViewModel(this Product product)
        {
            return new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Image = product.Image!= null ? System.Text.Encoding.UTF8.GetString(product.Image) : "",
                DateCreated = product.Created,
                Category = product.Category.MapToLookupViewModel()
            };
        }

    }
}
