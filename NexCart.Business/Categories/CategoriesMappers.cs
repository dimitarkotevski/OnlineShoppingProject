using NexCart.Business.ViewModels;
using NexCart.Domain;

namespace NexCart.Business.Mappers
{
    public static class CategoriesMappers
    {
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
