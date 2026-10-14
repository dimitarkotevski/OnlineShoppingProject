using IntellaQuest.BusinessLogic;
using IntellaQuest.BusinessLogic.ViewModels;
using IntellaQuest.Domain;

namespace IntellaQuest.BusinessLogic.Mappers
{
    public static class FavouriteProductsMappers
    {
        public static FavouriteProductsViewModel MapToViewModel(this FavouriteProducts model)
        {
            return new FavouriteProductsViewModel
            {
                Id = model.Id,
                User = new LookupViewModel
                {
                    Id = model.User.Id,
                    Name = model.User.FirstName + " " + model.User.LastName,
                },
                Product = new LookupViewModel
                {
                    Id = model.Product.Id,
                    Name = model.Product.Name,
                },
            };
        }
    }
}
