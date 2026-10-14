using IntellaQuest.BusinessLogic;
using IntellaQuest.BusinessLogic.ViewModels;
using IntellaQuest.Domain;
using System.Linq;

namespace IntellaQuest.BusinessLogic.Mappers
{
    public static class CategoriesMappers
    {
        public static CategoryViewModel MapToViewModel(this Category category)
        {
            return new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Status = category.Status,
                Products = category.Products.Select(
                    product=> new LookupViewModel { 
                        Id=product.Id ,
                        Name=product.Name,
                    }).ToList()
            };
        }
        public static CategoryLookupViewModel MapToLookupViewModel(this Category category)
        {
            return new CategoryLookupViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Status = category.Status,
                Url = category.Url,
            };
        }
    }
}
