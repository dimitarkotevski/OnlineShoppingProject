using FluentNHibernate.Utils;
using NexCart.Business.Exceptions;
using NexCart.Business.Exceptions.ExceptionMassages;
using NexCart.Business.Mappers;
using NexCart.Business.Requests;
using NexCart.Business.Responses;
using NexCart.Business.ViewModels;
using NexCart.Business.Mappers;
using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Data.NHibernate.Repositories;
using NexCart.Domain;
using NexCart.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Policy;

namespace NexCart.Business.Services
{
    public interface ICategoryService
    {
        CategoryViewModel Get(Guid Id);
        ResponseListModel<CategoryViewModel> GetAll(RequestModel request);
        List<CategoryLookupViewModel> GetAll();
        bool Create(CategoryViewModel model);
        bool Update(CategoryViewModel model);
        bool DeleteById(Guid Id);
        bool CheckCategoryNameExists(String Name);
        bool CheckCategoryStatus(bool status);
        ResponseListModel<CategoryViewModel> FilterAndPage(RequestModel request);
        ResponseListModel<ProductViewModel> GetProductsByCategory(Guid Id, RequestModel request);
        CategoryLookupViewModel GetCategoryById(Guid Id);
    }

    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IFavouriteProductsRepository _favouriteProductsRepository;
        private readonly IShoppingCartDetailRepository _shoppingCartDetailRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IOrderRepository orderRepository,
            IFavouriteProductsRepository favouriteProductsRepository,
            IShoppingCartDetailRepository shoppingCartDetailRepository,
            IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _orderRepository = orderRepository;
            _favouriteProductsRepository = favouriteProductsRepository;
            _shoppingCartDetailRepository = shoppingCartDetailRepository;
            _unitOfWork = unitOfWork;
        }


        public bool CheckCategoryNameExists(String Name)
        {
            return _categoryRepository.FilterBy(x => x.Name == Name).Any();
        }
        public bool CheckCategoryStatus(bool status)
        {
            return _categoryRepository.FilterBy(x => x.Status == status).ToList().Any();
        }

        public bool Create(CategoryViewModel model)
        {
            using (_unitOfWork.BeginTransaction())
            {
                if (CheckCategoryNameExists(model.Name))
                {
                    throw new BllException(ShopExceptionMassages.CategoriesExceptionMassages.NAME_ALREADY_EXIST_EXCEPTION);
                }
                var entity = new Category
                {
                    Name = model.Name,
                    Status = model.Status,
                };
                _categoryRepository.Add(entity);
                _unitOfWork.Commit();

                return true;
            }
        }
        public bool DeleteById(Guid Id)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var categoryEntity = _categoryRepository.FindBy(Id);
                if (_categoryRepository.FindBy(Id) == null)
                {
                    throw new BllException(ShopExceptionMassages.CategoriesExceptionMassages.NOT_FOUND_EXCEPTION);
                }
                _categoryRepository.Delete(categoryEntity);
                _unitOfWork.Commit();
                return true;
            }
        }

        public ResponseListModel<CategoryViewModel> FilterAndPage(RequestModel request)
        {
            using (_unitOfWork.BeginTransaction())
            {
                ResponseListModel<CategoryViewModel> response = new ResponseListModel<CategoryViewModel>();
                IQueryable<Category> listCategoryViewModelForFiltering = _categoryRepository.All();

                //Filter
                if (!string.IsNullOrEmpty(request.SearchString))
                {
                    listCategoryViewModelForFiltering = listCategoryViewModelForFiltering.Where(x => x.Name.Contains(request.SearchString));
                }
                if (!string.IsNullOrEmpty(request.SearchStatus))
                {
                    if (request.SearchStatus == "true")
                    {
                        listCategoryViewModelForFiltering = listCategoryViewModelForFiltering.Where(x => x.Status == true);
                    }
                    else
                    {
                        listCategoryViewModelForFiltering = listCategoryViewModelForFiltering.Where(x => x.Status == false);
                    }
                }

                
                //Sort
                switch (request.SortName)
                {
                    case "CategoryName":
                        if (string.IsNullOrEmpty(request.isAscending))
                        {
                            break;
                        }
                        else if (request.isAscending.Equals("asc"))
                        {
                            listCategoryViewModelForFiltering = listCategoryViewModelForFiltering.OrderBy(x => x.Name);
                        }
                        else
                        {
                            listCategoryViewModelForFiltering = listCategoryViewModelForFiltering.OrderByDescending(x => x.Name);
                        }
                        break;
                    case "CategoryStatus":
                        if (string.IsNullOrEmpty(request.isAscending))
                        {
                            break;
                        }
                        else if (request.isAscending.Equals("asc"))
                        {
                            listCategoryViewModelForFiltering = listCategoryViewModelForFiltering.OrderBy(x => x.Status);
                        }
                        else
                        {
                            listCategoryViewModelForFiltering = listCategoryViewModelForFiltering.OrderByDescending(x => x.Status);
                        }
                        break;
                    default:
                        break;
                }
                //Pagination
                if(request.Size == 0 && request.PageNeeded == 0)
                {
                    request.PageNeeded= 1;
                    request.Size = listCategoryViewModelForFiltering.Count();
                }
                response.TotalItems = listCategoryViewModelForFiltering.Count();

                return response;
            }
        }

        public CategoryViewModel Get(Guid Id)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var category = _categoryRepository.FindBy(Id);
                if (category == null)
                {
                    throw new BllException(ShopExceptionMassages.CategoriesExceptionMassages.NOT_FOUND_EXCEPTION);
                }
                _unitOfWork.Commit();

                return category.MapToViewModel();
            }
        }

        public ResponseListModel<CategoryViewModel> GetAll(RequestModel request)
        {
            return FilterAndPage(request);
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

        public bool Update(CategoryViewModel model)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var category = _categoryRepository.FindBy(model.Id);
                if (category == null)
                {
                    throw new BllException(ShopExceptionMassages.CategoriesExceptionMassages.NOT_FOUND_EXCEPTION);
                }
                if (_categoryRepository.CheckExist(x=>x.Id!=model.Id && x.Name==model.Name))
                {
                    throw new BllException(ShopExceptionMassages.CategoriesExceptionMassages.NAME_ALREADY_EXIST_EXCEPTION);
                }
                
                category.Status = model.Status;
                category.Name = model.Name;
                _categoryRepository.Update(category);

                _unitOfWork.Commit();
                return true;
            }
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
                    if(request.SearchBrands != null && request.SearchBrands.Count > 0)
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
                        queryCategoryProducts = queryCategoryProducts.Where(x=>x.Price>=request.PriceFrom && x.Price<request.PriceTo);
                    }

                }

                response.Items = queryCategoryProducts.Select(x => x.MapToViewModel()).ToList();
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
