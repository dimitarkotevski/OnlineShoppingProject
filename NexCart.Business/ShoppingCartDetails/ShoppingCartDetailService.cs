using NexCart.Business.Exceptions.ExceptionMassages;
using NexCart.Business.Exceptions;
using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Domain;
using NexCart.Data.NHibernate.Repositories;
using System;
using NexCart.Data.NHibernate.Repositories;

namespace NexCart.Business.Services
{
    public interface IShoppingCartDetailService
    {
        void AddShoppingCartDetailToShoppingCart(Guid userId, Guid productId, int quality);
        void RemoveProductFromShoppingCart(Guid shoppingCartDetailId);

    }
    public class ShoppingCartDetailService : IShoppingCartDetailService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly IShoppingCartDetailRepository _shoppingCartDetailRepository;
        private readonly IUserRepository _userRepository;
        private readonly IProductRepository _productRepository;
        public ShoppingCartDetailService(
            IShoppingCartDetailRepository shoppingCartDetailRepository,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            IProductRepository productRepository,
            IShoppingCartRepository shoppingCartRepository)
        {
            _shoppingCartDetailRepository = shoppingCartDetailRepository;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _productRepository = productRepository;
            _shoppingCartRepository = shoppingCartRepository;
        }

        public void AddShoppingCartDetailToShoppingCart(Guid userId, Guid productId, int quality)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var user = _userRepository.FindBy(userId)
                    ?? throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);

                var product = _productRepository.FindBy(productId)
                    ?? throw new BllException(ShopExceptionMassages.ProductsExceptionMassages.NOT_FOUND_EXCEPTION);

                var shoppingCart = _shoppingCartRepository.FindBy(x => x.User.Id == userId && x.Active == true);

                var exist = _shoppingCartDetailRepository.CheckExist(x =>
                    x.Product == product &&
                    x.ShoppingCart == shoppingCart &&
                    x.ShoppingCart.User == user);

                if (exist)
                {
                    var shoppingDetail = _shoppingCartDetailRepository.FindBy(x =>
                    x.Product == product &&
                    x.ShoppingCart == shoppingCart &&
                    x.ShoppingCart.User == user);

                    shoppingDetail.Quantity++;

                    _shoppingCartDetailRepository.Update(shoppingDetail);
                    _unitOfWork.Commit();
                }
                else
                {
                    var shoppingCartDetail = new ShoppingCartDetail
                    {
                        Product = product,
                        ShoppingCart = shoppingCart,
                        Quantity = quality
                    };

                    _shoppingCartDetailRepository.Add(shoppingCartDetail);

                    _unitOfWork.Commit();
                }
            }
        }
        public void RemoveProductFromShoppingCart(Guid id)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var detailCart = _shoppingCartDetailRepository.FindBy(id);
                if (detailCart == null)
                {
                    throw new BllException("Detail not exist");
                }

                _shoppingCartDetailRepository.Delete(detailCart);
                _unitOfWork.Commit();
            }
        }
    }
}
