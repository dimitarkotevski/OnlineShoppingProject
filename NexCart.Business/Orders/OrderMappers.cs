using NexCart.Business.ViewModels;
using NexCart.Business.Mappers;
using NexCart.Domain;
using System.Linq;

namespace NexCart.Business.Mappers
{
    public static class OrderMappers
    {
        public static OrderViewModel MapToViewModel(this Order model)
        {
            return new OrderViewModel
            {
                Id = model.Id,
                OrderName = model.OrderName,
                User = model.User != null ? model.User.MapToLookupUsernameViewModel() : null,
                ShoppingCart = model.ShoppingCart != null ? new LookupViewModel
                {
                    Id = model.ShoppingCart.Id,
                    Name = model.ShoppingCart.Name,
                    Status = true
                } : null,
                OrderStatus = model.OrderStatus,
                // The total is the historical snapshot, not a value recalculated from
                // today's catalogue prices.
                TotalAmount = model.TotalAmount,
                PaymentType = model.PaymentType,
                DateCreated = model.DateCreated
            };
        }

        public static OrderGridViewModel MapToGridViewModel(this Order model)
        {
            return new OrderGridViewModel
            {
                Id = model.Id,
                OrderName = model.OrderName,
                NumberOfProducts = (model.OrderItems ?? Enumerable.Empty<OrderItem>()).Count(),
                OrderStatus = model.OrderStatus,
                TotalAmount = model.TotalAmount,
                DateCreated = model.DateCreated,
                PaymentType = model.PaymentType
            };
        }

        public static OrderViewModelWithProducts MapToViewModelWithProducts(this Order model)
        {
            return new OrderViewModelWithProducts
            {
                Id = model.Id,
                OrderName = model.OrderName,
                OrderStatus = model.OrderStatus,
                Products = (model.OrderItems ?? Enumerable.Empty<OrderItem>())
                    .Select(x => x.MapToViewModel())
                    .ToList(),
                TotalAmount = model.TotalAmount,
                ShoppingCart = model.ShoppingCart != null ? new LookupViewModel
                {
                    Id = model.ShoppingCart.Id,
                    Name = model.ShoppingCart.Name,
                    Status = true
                } : null,
                PaymentType = model.PaymentType
            };
        }

        /// <summary>
        /// Projects an immutable order line into the same shape the storefront already uses
        /// for cart lines. Name and price come from the snapshot, not the live product.
        /// </summary>
        public static ShoppingCartDetailViewModel MapToViewModel(this OrderItem item)
        {
            return new ShoppingCartDetailViewModel
            {
                Id = item.Id,
                Quantity = item.Quantity,
                Product = new ProductViewModel
                {
                    Id = item.Product?.Id,
                    Name = item.ProductName,
                    Price = item.UnitPrice,
                    Description = item.Product?.Description,
                    ImageUrl = item.Product?.ImageUrl,
                    Image = item.Product?.Image ?? "",
                    Category = item.Product?.Category != null
                        ? item.Product.Category.MapToLookupViewModel()
                        : null
                }
            };
        }
    }
}
