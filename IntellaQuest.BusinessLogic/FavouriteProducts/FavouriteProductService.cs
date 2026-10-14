using IntellaQuest.BusinessLogic.Exceptions.ExceptionMassages;
using IntellaQuest.BusinessLogic.Exceptions;
using IntellaQuest.BusinessLogic.Responses;
using IntellaQuest.BusinessLogic.ViewModels;
using IntellaQuest.Domain;
using IntellaQuest.Data.NHibernate.Repositories;
using System;
using IntellaQuest.Data.NHibernate.Repositories;
using IntellaQuest.Data.NHibernate.ConfigurationRepository;
using System.Linq;
using IntellaQuest.BusinessLogic.Mappers;

namespace IntellaQuest.BusinessLogic.Services
{
    public interface IFavouriteProductService
    {
        void AddProductToFavouriteProduct(Guid userId, Guid productId);
        void RemoveProductFromFavuriteProduct(Guid userId, Guid productId);
        ResponseListModel<ProductViewModel> GetUserFavouriteProducts(Guid userId);
    }
    public class FavouriteProductService : IFavouriteProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IFavouriteProductsRepository _favouriteProductsRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        public FavouriteProductService(IFavouriteProductsRepository favouriteProductsRepository, IUserRepository userRepository, IUnitOfWork unitOfWork, IProductRepository productRepository)
        {
            _unitOfWork = unitOfWork;

            _favouriteProductsRepository = favouriteProductsRepository;
            _userRepository = userRepository;
            _productRepository = productRepository;
        }

        public ResponseListModel<ProductViewModel> GetUserFavouriteProducts(Guid userId)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var user = _userRepository.FindBy(userId)
                    ?? throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);

                var userFavouriteProducts = _favouriteProductsRepository.GetUserFavouriteProducts(user)
                    ?? throw new BllException(ShopExceptionMassages.FavouriteProductExceptionMassages.NOT_FOUND_EXCEPTION); ;

                return new ResponseListModel<ProductViewModel>
                {
                    TotalItems = userFavouriteProducts.Count(),
                    Items = userFavouriteProducts.Select(x => x.Product.MapToViewModel()).ToList(),
                };
            }
        }
        public void AddProductToFavouriteProduct(Guid userId, Guid productId)
        {
            var user = _userRepository.FindBy(userId);
            if (user == null)
                throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);
            var product = _productRepository.FindBy(productId);
            if (product == null)
                throw new BllException(ShopExceptionMassages.ProductsExceptionMassages.NOT_FOUND_EXCEPTION);

            var favouriteProductExist = _favouriteProductsRepository.CheckExist(x => x.Product.Id == productId && x.User.Id == userId);
            if (favouriteProductExist)
                throw new BllException(ShopExceptionMassages.FavouriteProductExceptionMassages.NOT_FOUND_EXCEPTION);

            using (_unitOfWork.BeginTransaction())
            {

                var favouriteProduct = new FavouriteProducts
                {
                    User = user,
                    Product = product
                };
                _favouriteProductsRepository.Add(favouriteProduct);
                _unitOfWork.Commit();
            }
        }
        public void RemoveProductFromFavuriteProduct(Guid userId, Guid productId)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var user = _userRepository.FindBy(userId)
                    ?? throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);

                var product = _productRepository.FindBy(productId)
                    ?? throw new BllException(ShopExceptionMassages.ProductsExceptionMassages.NOT_FOUND_EXCEPTION);

                var favouriteProduct = _favouriteProductsRepository.FindBy(x => x.Product == product && x.User == user);

                _favouriteProductsRepository.Delete(favouriteProduct);

                _unitOfWork.Commit();
            }
        }
    }
}
