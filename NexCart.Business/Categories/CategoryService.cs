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
        /// <param name="activeOnly">true for the storefront navbar, which hides inactive categories.</param>
        List<CategoryLookupViewModel> GetAll(bool activeOnly = false);
        ResponseListModel<ProductViewModel> GetProductsByCategory(Guid Id, RequestModel request);
        CategoryLookupViewModel GetCategoryById(Guid Id);

        // Admin tool
        ResponseModel<CategoryGridModel> GetTable(RequestModel request);
        void Create(CategoryViewModel model);
        void Update(CategoryViewModel model);
        /// <summary>Refuses to delete a category that still has products (they would be cascade-deleted).</summary>
        void Delete(Guid id);
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

        public List<CategoryLookupViewModel> GetAll(bool activeOnly = false)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var query = _categoryRepository.All();
                if (activeOnly)
                {
                    query = query.Where(category => category.Status);
                }
                var categories = query.ToList();
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

        #region ADMIN

        public ResponseModel<CategoryGridModel> GetTable(RequestModel request)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var query = _categoryRepository.All();

                if (!string.IsNullOrWhiteSpace(request.SearchString))
                {
                    var search = request.SearchString.Trim();
                    query = query.Where(category => category.Name.Contains(search));
                }
                if (request.IsActive.HasValue)
                {
                    var isActive = request.IsActive.Value;
                    query = query.Where(category => category.Status == isActive);
                }

                var categories = query.ToList();

                // Product ids per category, loaded as plain ids and counted in memory.
                var productCounts = _categoryRepository.All()
                    .SelectMany(category => category.Products)
                    .Select(product => product.Category.Id)
                    .ToList()
                    .GroupBy(categoryId => categoryId)
                    .ToDictionary(group => group.Key, group => group.Count());

                var size = request.Size > 0 ? request.Size : Math.Max(categories.Count, 1);
                var page = request.PageNeeded > 0 ? request.PageNeeded : 1;

                return new ResponseModel<CategoryGridModel>
                {
                    TotalItems = categories.Count,
                    Size = size,
                    CurrentPage = page,
                    Items = categories
                        .OrderBy(category => category.Name, StringComparer.OrdinalIgnoreCase)
                        .Skip((page - 1) * size)
                        .Take(size)
                        .Select(category => new CategoryGridModel
                        {
                            Id = category.Id,
                            Name = category.Name,
                            Status = category.Status,
                            Url = category.Url,
                            ProductCount = productCounts.GetValueOrDefault(category.Id)
                        })
                        .ToList()
                };
            }
        }

        public void Create(CategoryViewModel model)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var name = ValidateName(model, excludeId: null);

                _categoryRepository.Add(new Category
                {
                    Name = name,
                    Status = model.Status,
                    Url = ToSlug(name)
                });
                _unitOfWork.Commit();
            }
        }

        public void Update(CategoryViewModel model)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var category = _categoryRepository.FindBy(model.Id)
                    ?? throw new BllException(ShopExceptionMassages.CategoriesExceptionMassages.NOT_FOUND_EXCEPTION);
                var name = ValidateName(model, excludeId: model.Id);

                category.Name = name;
                category.Status = model.Status;
                category.Url = ToSlug(name);
                _categoryRepository.Update(category);
                _unitOfWork.Commit();
            }
        }

        public void Delete(Guid id)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var category = _categoryRepository.FindBy(id)
                    ?? throw new BllException(ShopExceptionMassages.CategoriesExceptionMassages.NOT_FOUND_EXCEPTION);

                // The Products mapping cascades deletes, so removing a non-empty category would
                // silently delete its whole catalog.
                var productCount = _categoryRepository.GetProductsByCategory(category).Count();
                if (productCount > 0)
                {
                    throw new BllException(string.Format(
                        ShopExceptionMassages.CategoriesExceptionMassages.HAS_PRODUCTS_EXCEPTION,
                        category.Name, productCount));
                }

                _categoryRepository.Delete(category);
                _unitOfWork.Commit();
            }
        }

        /// <summary>Trimmed name; throws when it is empty or used by another category.</summary>
        private string ValidateName(CategoryViewModel model, Guid? excludeId)
        {
            var name = model?.Name?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                throw new BllException(ShopExceptionMassages.CategoriesExceptionMassages.NAME_REQUIRED_EXCEPTION);
            }

            var duplicate = _categoryRepository.All()
                .Where(category => category.Name == name)
                .Select(category => category.Id)
                .ToList()
                .Any(id => id != excludeId);
            if (duplicate)
            {
                throw new BllException(ShopExceptionMassages.CategoriesExceptionMassages.NAME_ALREADY_EXIST_EXCEPTION);
            }

            return name;
        }

        /// <summary>"Headphones &amp; Audio" -> "headphones-audio".</summary>
        private static string ToSlug(string name)
        {
            var chars = name.ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
            return string.Join("-", new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
        }

        #endregion
    }
}
