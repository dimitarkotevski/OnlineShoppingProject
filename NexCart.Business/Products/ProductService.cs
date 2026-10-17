using System.Linq;
using NexCart.Business.Mappers;
using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Data.NHibernate.Repositories;
using System;
using NexCart.Business.Exceptions;
using NexCart.Business.Exceptions.ExceptionMassages;
using NexCart.Domain;
using NexCart.Business.Responses;
using NexCart.Business.Requests;
using System.Collections.Generic;
using NexCart.Business.ViewModels;
using System.Drawing;
using System.Data;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NexCart.Data.NHibernate.Repositories;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace NexCart.Business.Services
{
    public interface IProductService
    {
        ResponseModel<ProductViewModel> GetAll(RequestModel request);
        ProductViewModel Get(Guid Id);
        bool Create(ProductViewModel model);
        bool Update(ProductViewModel model);
        bool Delete(Guid Id);
        bool CheckNameExists(string Name);
        void UploadImage(Guid Id, string image);
        List<string> GetAllBrands(Guid categoryId);
    }
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;


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
            IShoppingCartDetailRepository shoppingCartDetailRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            _productsRepository = productsRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _orderRepository = orderRepository;
            _favouriteProductsRepository = favouriteProductsRepository;
            _shoppingCartDetailRepository = shoppingCartDetailRepository;
            _webHostEnvironment = webHostEnvironment;
        }
        #region ADMIN CRUD

        private string SaveImageToFile(string base64Image)
        {
            if (string.IsNullOrEmpty(base64Image)) return null;

            try
            {
                // Remove Base64 header if present (e.g., "data:image/jpeg;base64,")
                string base64Data = base64Image.Contains(",")
                    ? base64Image.Split(',')[1]
                    : base64Image;

                byte[] imageBytes = Convert.FromBase64String(base64Data);
                string fileName = $"{Guid.NewGuid()}.jpg";
                string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string filePath = Path.Combine(folderPath, fileName);
                File.WriteAllBytes(filePath, imageBytes);

                return $"/images/products/{fileName}";
            }
            catch (Exception)
            {
                return null;
            }
        }

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

                var entity = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    Category = category,
                    Created = DateTime.Now,
                    Image = model.Image != null ? SaveImageToFile(model.Image) : null
                };

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

                product.Name = model.Name;
                product.Price = model.Price;
                product.Description = model.Description;
                product.Category = category;

                if (model.Image != null)
                {
                    product.Image = SaveImageToFile(model.Image);
                }

                _productsRepository.Update(product);
                _unitOfWork.Commit();

                return true;
            }
        }
        public void UploadImage(Guid Id, string image)
        {
            using (_unitOfWork.BeginTransaction())
            {

                var product = _productsRepository.FindBy(Id)
                    ?? throw new BllException(ShopExceptionMassages.ProductsExceptionMassages.NOT_FOUND_EXCEPTION);

                product.Image = SaveImageToFile(image);

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
