using NexCart.Business.ViewModels;
using NexCart.Business.Mappers;
using NexCart.Domain;
using System;
using System.Linq;

namespace NexCart.Business.Mappers
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
                ImageUrl = product.ImageUrl,
                Image = product.Image ?? "",
                DateCreated = product.Created,
                Category = product.Category.MapToLookupViewModel()
            };
        }

    }
}
