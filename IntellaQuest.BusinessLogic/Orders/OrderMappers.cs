using IntellaQuest.BusinessLogic.ViewModels;
using IntellaQuest.BusinessLogic.Mappers;
using IntellaQuest.Domain;
using System.Linq;
using System.Reflection;

namespace IntellaQuest.BusinessLogic.Mappers
{
    public static class OrderMappers
    {
        
        public static OrderViewModel MapToViewModel(this Order model)
        {
            var sumProducts = model.ShoppingCart.ShoppingCartDetails.Select(x => x.Product.Price).Sum();
            return new OrderViewModel
            {
                Id=model.Id,
                OrderName= model.OrderName,
                User = model.ShoppingCart.User.MapToLookupUsernameViewModel(),
                ShoppingCart = new LookupViewModel
                {
                    Id = model.ShoppingCart.Id,
                    Name = model.ShoppingCart.Name,
                    Status = true
                },
                OrderStatus = model.OrderStatus,
                TotalAmount = sumProducts == model.TotalAmount ? model.TotalAmount : sumProducts,
                PaymentType = model.PaymentType,
                DateCreated = model.DateCreated
            };
        }
        public static OrderGridViewModel MapToGridViewModel(this Order model)
        {
            var sumProducts = model.ShoppingCart.ShoppingCartDetails.Select(x => x.Product.Price).Sum();
            return new OrderGridViewModel
            {
                Id = model.Id,
                OrderName = model.OrderName,
                NumberOfProducts = model.ShoppingCart.ShoppingCartDetails.Select(x=>x.Product).Count(),
                OrderStatus = model.OrderStatus,
                TotalAmount = sumProducts == model.TotalAmount ? model.TotalAmount : sumProducts,
                DateCreated = model.DateCreated,
                PaymentType = model.PaymentType
            };
        }

        public static OrderViewModelWithProducts MapToViewModelWithProducts(this Order model)
        {
            //var sumProducts = model.ShoppingCart.ShoppingCartDetails.Select(x => x.Product.Price).Sum();

            return new OrderViewModelWithProducts
            {
                Id = model.Id,
                OrderName = model.OrderName,
                OrderStatus = model.OrderStatus,
                Products = model.ShoppingCart.ShoppingCartDetails.Select(x => x.MapToViewModel()).ToList(),
                TotalAmount =model.TotalAmount,
                ShoppingCart = model.ShoppingCart != null ? new LookupViewModel
                {
                    Id = model.ShoppingCart.Id,
                    Name = model.ShoppingCart.Name,
                    Status = true
                } : null,
                PaymentType = model.PaymentType
            };
        }
    }
}
