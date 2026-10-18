using NexCart.Business.Exceptions;
using NexCart.Business.Exceptions.ExceptionMassages;
using NexCart.Business.Mappers;
using NexCart.Business.Requests;
using NexCart.Business.Responses;
using NexCart.Business.ViewModels;
using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Data.NHibernate.Repositories;
using NexCart.Domain;
using NexCart.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NexCart.Business.Services
{
    public interface ICategoryService
    {
        List<CategoryLookupViewModel> GetAll();
        ResponseListModel<ProductViewModel> GetProductsByCategory(Guid Id, RequestModel request);
        CategoryLookupViewModel GetCategoryById(Guid Id);
    }

    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IFavouriteProductsRepository _favouriteProductsRepository;
        private readonly IShoppingCartDetailRepository _shoppingCartDetailRepository;
        private readonly IProductSortService _productSortService;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IOrderRepository orderRepository,
            IFavouriteProductsRepository favouriteProductsRepository,
            IShoppingCartDetailRepository shoppingCartDetailRepository,
            IProductSortService productSortService,
            IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _orderRepository = orderRepository;
            _favouriteProductsRepository = favouriteProductsRepository;
            _shoppingCartDetailRepository = shoppingCartDetailRepository;
            _productSortService = productSortService;
            _unitOfWork = unitOfWork;
        }

        public List<CategoryLookupViewModel> GetAll()
        {
            using (_unitOfWork.BeginTransaction())
            {
                var categories = _categoryRepository.All().ToList();
                var popularity = GetCategoryPopularityScores();

                // Popular categories first (most ordered / most favourited products), then
                // alphabetically so the order stays sensible and stable for everything else.
                return categories
                    .OrderByDescending(category => popularity.TryGetValue(category.Id, out var score) ? score : 0)
                    .ThenBy(category => category.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(category => category.MapToLookupViewModel())
                    .ToList();
            }
        }

        /// <summary>
        /// Scores every category by the real demand for its products: the number of units that
        /// were actually ordered (every order that is not canceled) plus the number of times any
        /// of its products was favourited. Categories with no activity are absent from the result
        /// (score 0) and therefore fall back to alphabetical order in the navbar.
        /// </summary>
        private Dictionary<Guid, int> GetCategoryPopularityScores()
        {
            // Only products sitting in a cart that became a real (non-canceled) order count.
            var orderedCartIds = _orderRepository.All()
                .Where(order => order.OrderStatus != OrderStatus.Canceled)
                .Select(order => order.ShoppingCart.Id)
                .ToList()
                .ToHashSet();

            var orderedDetails = _shoppingCartDetailRepository.All()
                .Select(detail => new
                {
                    CartId = detail.ShoppingCart.Id,
                    CategoryId = detail.Product.Category.Id,
                    detail.Quantity
                })
                .ToList();

            var favouriteCategoryIds = _favouriteProductsRepository.All()
                .Select(favourite => favourite.Product.Category.Id)
                .ToList();

            var scores = new Dictionary<Guid, int>();

            foreach (var detail in orderedDetails.Where(detail => orderedCartIds.Contains(detail.CartId)))
            {
                scores[detail.CategoryId] = scores.TryGetValue(detail.CategoryId, out var ordered)
                    ? ordered + detail.Quantity
                    : detail.Quantity;
            }

            foreach (var categoryId in favouriteCategoryIds)
            {
                scores[categoryId] = scores.TryGetValue(categoryId, out var favourites)
                    ? favourites + 1
                    : 1;
            }

            return scores;
        }

        public ResponseListModel<ProductViewModel> GetProductsByCategory(Guid Id, RequestModel request)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var category = _categoryRepository.FindBy(Id)
                    ?? throw new BllException(ShopExceptionMassages.CategoriesExceptionMassages.NOT_FOUND_EXCEPTION);

                ResponseListModel<ProductViewModel> response = new ResponseListModel<ProductViewModel>();
                var queryCategoryProducts = _categoryRepository.GetProductsByCategory(category);

                if (request != null)
                {
                    if (request.SearchBrands != null && request.SearchBrands.Count > 0)
                    {
                        IQueryable<Product> productsToJoin = Enumerable.Empty<Product>().AsQueryable();

                        foreach (var search in request.SearchBrands)
                        {
                            var searchResult = queryCategoryProducts
                                                .Where(x =>
                                                    x.Name.Contains(search));
                            productsToJoin = productsToJoin.Union(searchResult);
                        }

                        queryCategoryProducts = productsToJoin;
                    }
                    if (request.SearchList != null && request.SearchList.Count > 0)
                    {
                        IQueryable<Product> productsToJoin = Enumerable.Empty<Product>().AsQueryable();

                        foreach (var search in request.SearchList)
                        {
                            var searchResult = queryCategoryProducts
                                                .Where(x =>
                                                    x.Description.Contains(search) ||
                                                    x.Tags.Contains(search));
                            productsToJoin = productsToJoin.Union(searchResult);
                        }

                        queryCategoryProducts = productsToJoin;

                    }

                    if ((request.PriceFrom != null && request.PriceTo != null) && (request.PriceFrom < request.PriceTo))
                    {
                        queryCategoryProducts = queryCategoryProducts.Where(x => x.Price >= request.PriceFrom && x.Price < request.PriceTo);
                    }

                }

                response.Items = _productSortService.Sort(queryCategoryProducts.ToList(), request?.SortName)
                    .Select(x => x.MapToViewModel())
                    .ToList();
                response.TotalItems = response.Items.Count();

                return response;
            }
        }

        public CategoryLookupViewModel GetCategoryById(Guid Id)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var category = _categoryRepository.FindBy(Id)
                   ?? throw new BllException(ShopExceptionMassages.CategoriesExceptionMassages.NOT_FOUND_EXCEPTION);

                return category.MapToLookupViewModel();
            }
        }
    }
}
