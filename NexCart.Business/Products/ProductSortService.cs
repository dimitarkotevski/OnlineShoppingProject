using NexCart.Data.NHibernate.Repositories;
using NexCart.Domain;
using NexCart.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NexCart.Business.Services
{
    /// <summary>
    /// Sort orders offered by the storefront sorting bar (home page and category pages).
    /// The values match the SortName the Angular client sends.
    /// </summary>
    public static class ProductSortNames
    {
        public const string Popular = "Popular";
        public const string Newest = "Newest";
        public const string BestSelling = "BestSelling";
        public const string PriceLow = "PriceLow";
        public const string PriceHigh = "PriceHigh";
    }

    public interface IProductSortService
    {
        /// <summary>
        /// Orders already-filtered products by <paramref name="sortName"/>. Unknown or empty sort
        /// names keep the incoming order. Must be called inside an open unit of work.
        /// </summary>
        List<Product> Sort(IEnumerable<Product> products, string sortName);
    }

    public class ProductSortService : IProductSortService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IFavouriteProductsRepository _favouriteProductsRepository;
        private readonly IShoppingCartDetailRepository _shoppingCartDetailRepository;

        public ProductSortService(
            IOrderRepository orderRepository,
            IFavouriteProductsRepository favouriteProductsRepository,
            IShoppingCartDetailRepository shoppingCartDetailRepository)
        {
            _orderRepository = orderRepository;
            _favouriteProductsRepository = favouriteProductsRepository;
            _shoppingCartDetailRepository = shoppingCartDetailRepository;
        }

        public List<Product> Sort(IEnumerable<Product> products, string sortName)
        {
            switch (sortName)
            {
                case ProductSortNames.Popular:
                    return ByScoreThenNewest(products, GetFavouriteCounts());
                case ProductSortNames.BestSelling:
                    return ByScoreThenNewest(products, GetSoldQuantities());
                case ProductSortNames.Newest:
                    return products.OrderByDescending(p => p.Created).ToList();
                case ProductSortNames.PriceLow:
                    return products.OrderBy(p => p.Price).ToList();
                case ProductSortNames.PriceHigh:
                    return products.OrderByDescending(p => p.Price).ToList();
                default:
                    return products.ToList();
            }
        }

        /// <summary>Highest score first; products with equal (or no) score fall back to newest first.</summary>
        private static List<Product> ByScoreThenNewest(IEnumerable<Product> products, Dictionary<Guid, int> scores)
        {
            return products
                .OrderByDescending(p => scores.GetValueOrDefault(p.Id))
                .ThenByDescending(p => p.Created)
                .ToList();
        }

        /// <summary>How many users favourited each product.</summary>
        private Dictionary<Guid, int> GetFavouriteCounts()
        {
            return _favouriteProductsRepository.All()
                .Select(favourite => favourite.Product.Id)
                .ToList()
                .GroupBy(productId => productId)
                .ToDictionary(group => group.Key, group => group.Count());
        }

        /// <summary>
        /// Units sold per product. Only products in a cart that became a real (non-canceled)
        /// order count; carts that were never checked out are not sales.
        /// </summary>
        private Dictionary<Guid, int> GetSoldQuantities()
        {
            var orderedCartIds = _orderRepository.All()
                .Where(order => order.OrderStatus != OrderStatus.Canceled)
                .Select(order => order.ShoppingCart.Id)
                .ToList()
                .ToHashSet();

            return _shoppingCartDetailRepository.All()
                .Select(detail => new { CartId = detail.ShoppingCart.Id, ProductId = detail.Product.Id, detail.Quantity })
                .ToList()
                .Where(detail => orderedCartIds.Contains(detail.CartId))
                .GroupBy(detail => detail.ProductId)
                .ToDictionary(group => group.Key, group => group.Sum(detail => detail.Quantity));
        }
    }
}
