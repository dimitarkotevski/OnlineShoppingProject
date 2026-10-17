using System.Linq;
using IntellaQuest.BusinessLogic.Mappers;
using IntellaQuest.Data.NHibernate.ConfigurationRepository;
using IntellaQuest.Data.NHibernate.Repositories;
using System;
using IntellaQuest.BusinessLogic.Exceptions;
using IntellaQuest.BusinessLogic.Exceptions.ExceptionMassages;
using IntellaQuest.Domain;
using IntellaQuest.BusinessLogic.Responses;
using IntellaQuest.BusinessLogic.Requests;
using System.Collections.Generic;
using IntellaQuest.BusinessLogic.ViewModels;
using System.Drawing;
using System.Data;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using IntellaQuest.Data.NHibernate.Repositories;
using static System.Net.Mime.MediaTypeNames;

namespace IntellaQuest.BusinessLogic.Services
{
    public interface IProductService
    {
        ResponseModel<ProductViewModel> GetAll(RequestModel request);
        ProductViewModel Get(Guid Id);
        bool Create(ProductViewModel model);
        bool Update(ProductViewModel model);
        bool Delete(Guid Id);
        string GetProductImage(Guid Id);
        bool CheckNameExists(string Name);
        List<ProductViewModel> GetAllTable();
        void UploadImage(Guid Id, string image);
        List<string> GetAllBrands(Guid categoryId);
    }
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IProductRepository _productsRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IFavouriteProductsRepository _favouriteProductsRepository;
        private readonly IShoppingCartDetailRepository _shoppingCartDetailRepository;

        public ProductService(IProductRepository productsRepository, 
            ICategoryRepository categoryRepository, 
            IUnitOfWork unitOfWork, 
            IOrderRepository orderRepository,
            IFavouriteProductsRepository favouriteProductsRepository,
            IShoppingCartDetailRepository shoppingCartDetailRepository)
        {
            _productsRepository = productsRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _orderRepository = orderRepository;
            _favouriteProductsRepository = favouriteProductsRepository;
            _shoppingCartDetailRepository = shoppingCartDetailRepository;
        }
        #region ADMIN CRUD

        public bool CheckNameExists(string Name)
        {
            return _productsRepository.CheckExist(x => x.Name == Name);
        }
        public bool Create(ProductViewModel model)
        {
            using (_unitOfWork.BeginTransaction())
            {
                if (_productsRepository.CheckExist(x => x.Name == model.Name))
                {
                    throw new BllException(ShopExceptionMassages.ProductsExceptionMassages.NAME_ALREADY_EXIST_EXCEPTION);
                }
                var category = _categoryRepository.FindBy(model.Category.Id);
                if (category == null)
                {
                    throw new BllException(ShopExceptionMassages.CategoriesExceptionMassages.NOT_FOUND_EXCEPTION);
                }

                string base64String = null;
                if (model.Image!= null)
                {
                    base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(model.Image));
                }

                var entity = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    Category = category,
                    Created = DateTime.Now
                };
                if(base64String != null)
                {
                    entity.Image = Convert.FromBase64String(base64String);
                }

                _productsRepository.Add(entity);
                _unitOfWork.Commit();

                return true;
            }
        }
        public bool Delete(Guid Id)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var productEntity = _productsRepository.FindBy(Id);
                if (productEntity == null)
                {
                    throw new BllException(ShopExceptionMassages.ProductsExceptionMassages.NOT_FOUND_EXCEPTION);
                }
                _productsRepository.Delete(productEntity);
                _unitOfWork.Commit();
                return true;
            }
        }
        public ProductViewModel Get(Guid Id)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var productEntity = _productsRepository.FindBy(Id);
                if (productEntity == null)
                {
                    throw new BllException(ShopExceptionMassages.ProductsExceptionMassages.NOT_FOUND_EXCEPTION);
                }

                return productEntity.MapToViewModel();
            }
        }
        public bool Update(ProductViewModel model)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var category = _categoryRepository.FindBy(model.Category.Id);
                if (category == null)
                {
                    throw new BllException(ShopExceptionMassages.CategoriesExceptionMassages.NOT_FOUND_EXCEPTION);
                }
                var product = _productsRepository.FindBy(model.Id.Value);

                if (product == null)
                {
                    throw new BllException(ShopExceptionMassages.ProductsExceptionMassages.NOT_FOUND_EXCEPTION);
                }
                if (_productsRepository.CheckExist(x => x.Id != model.Id && x.Name == model.Name))
                {
                    throw new BllException(ShopExceptionMassages.CategoriesExceptionMassages.NAME_ALREADY_EXIST_EXCEPTION);
                }

                string base64String = null;
                if (model.Image != null)
                {
                    base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(model.Image));
                }

                //exception need
                product.Name = model.Name;
                product.Price = model.Price;
                if(base64String!= null)
                {
                    product.Image = Convert.FromBase64String(base64String);
                }
                product.Description = model.Description;
                product.Category = category;
                _productsRepository.Update(product);
                _unitOfWork.Commit();

                return true;
            }
        }
        public List<ProductViewModel> GetAllTable()
        {
            using (_unitOfWork.BeginTransaction())
            {
                return _productsRepository.All().Select(x => x.MapToViewModel()).ToList();
            }
        }
        public string GetProductImage(Guid Id)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var product = _productsRepository.FindBy(Id);

                return null;
            }
        }
        public void UploadImage(Guid Id, string image)
        {
            using (_unitOfWork.BeginTransaction())
            {

                var product = _productsRepository.FindBy(Id)
                    ?? throw new BllException(ShopExceptionMassages.ProductsExceptionMassages.NOT_FOUND_EXCEPTION);

                string base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(image));

                product.Image = Convert.FromBase64String(base64String);

                _productsRepository.Update(product);
                _unitOfWork.Commit();
            }
        }

        #endregion

        #region USER
        private ResponseModel<ProductViewModel> FilterAndPage(RequestModel request)
        {
            using (_unitOfWork.BeginTransaction())
            {
                ResponseModel<ProductViewModel> response = new ResponseModel<ProductViewModel>();

                IQueryable<Product> listProductForFiltering;
                
                listProductForFiltering = _productsRepository.All();

                if (!string.IsNullOrEmpty(request.SearchString))
                {
                    listProductForFiltering = listProductForFiltering.Where(x => x.Name.Contains(request.SearchString));
                }
                if (request.EntityId.HasValue)
                {
                    var category = _categoryRepository.FindBy(request.EntityId.Value);
                    if (category == null)
                    {
                        throw new BllException(ShopExceptionMassages.CategoriesExceptionMassages.NOT_FOUND_EXCEPTION);
                    }
                    listProductForFiltering = listProductForFiltering.Where(x => x.Category == category);
                }

                // Price Filtering
                if (request.PriceFrom.HasValue)
                {
                    listProductForFiltering = listProductForFiltering.Where(x => x.Price >= request.PriceFrom.Value);
                }
                if (request.PriceTo.HasValue)
                {
                    listProductForFiltering = listProductForFiltering.Where(x => x.Price <= request.PriceTo.Value);
                }

                // Brand Filtering (Brands are assumed to be the first word of the Product Name)
                if (request.SearchBrands != null && request.SearchBrands.Any())
                {
                    listProductForFiltering = listProductForFiltering.Where(x =>
                        request.SearchBrands.Any(brand => x.Name.StartsWith(brand)));
                }

                // SearchList Filtering
                if (request.SearchList != null && request.SearchList.Any())
                {
                    listProductForFiltering = listProductForFiltering.Where(x =>
                        request.SearchList.Any(item => x.Name.Contains(item)));
                }

                switch (request.SortName)
                {
                    case "Popular":
                        listProductForFiltering = _favouriteProductsRepository.All().Select(x => x.Product);
                        break;
                    case "BestSelling":
                        listProductForFiltering = _shoppingCartDetailRepository.All().Select(x => x.Product);
                        break;
                    case "Newest":
                        listProductForFiltering = listProductForFiltering.OrderByDescending(x => x.Created);
                        break;
                    case "PriceLow":
                        listProductForFiltering = listProductForFiltering.OrderBy(x => x.Price);
                        break;
                    case "PriceHigh":
                        listProductForFiltering = listProductForFiltering.OrderByDescending(x => x.Price);
                        break;
                    default:
                        break;
                }
                var list = listProductForFiltering.ToList().Distinct();

                if (request.Size == 0 && request.PageNeeded == 0)
                {
                    request.PageNeeded = 1;
                    request.Size = list.Count();
                }
                response.Size = request.Size;
                response.CurrentPage = request.PageNeeded;
                response.Items = list
                                    .Skip((response.CurrentPage - 1) * response.Size)
                                    .Take(response.Size).Select(x => x.MapToViewModel()).ToList();
                response.TotalItems = list.Count();


                return response;
            }
        }
        public ResponseModel<ProductViewModel> GetAll(RequestModel request)
        {   
            return FilterAndPage(request);
        }

        public List<string> GetAllBrands(Guid categoryId)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var category = _categoryRepository.FindBy(categoryId);
                var listOfBrands = _categoryRepository.GetProductsByCategory(category)
                    .Select(x => x.Name.Split(' ')[0]) // Get the first word of the name
                    .ToList();

                var distinctBrands = listOfBrands.Distinct().ToList();

                return distinctBrands;
            }
        }
        #endregion
    }
}
