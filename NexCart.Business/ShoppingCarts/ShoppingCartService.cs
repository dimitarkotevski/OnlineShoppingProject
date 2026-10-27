using NexCart.Business.Exceptions.ExceptionMassages;
using NexCart.Business.Exceptions;
using NexCart.Business.ViewModels;
using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Domain;
using System;
using NexCart.Data.NHibernate.Repositories;
using NexCart.Data.NHibernate.Repositories;
using NexCart.Business.Mappers;
using System.Collections.Generic;
using FluentNHibernate.Conventions;

namespace NexCart.Business.Services
{
    public interface IShoppingCartService
    {
        #region ShoppingCart
        ShoppingCartViewModel GetUserShoppingCartWithProducts(Guid userId);
        void AddProductToShoppingCart(Guid userId, Guid productId, int quality);
        bool DoesCartBelongToUser(Guid shoppingCartId, Guid userId);
        #endregion
        #region ShoppingCartDetails Action
        void RemoveProductFromShoppingCart(Guid shoppingCartDetailId);
        bool DoesDetailBelongToUser(Guid shoppingCartDetailId, Guid userId);
        #endregion
        void PlusQuantity(Guid detailId);
        void MinusQuantity(Guid detailId);
    }
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly IUserRepository _userRepository;
        private readonly IProductRepository _productRepository;
        private readonly IShoppingCartDetailRepository _shoppingCartDetailRepository;
        public ShoppingCartService(
            IShoppingCartRepository shoppingCartRepository,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            IProductRepository productRepository,
            IShoppingCartDetailRepository shoppingCartDetailRepository)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _productRepository = productRepository;
            _shoppingCartDetailRepository = shoppingCartDetailRepository;
        }


        public ShoppingCartViewModel GetUserShoppingCartWithProducts(Guid userId)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var user = _userRepository.FindBy(userId)
                    ?? throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);

                var userShoppingCart = _shoppingCartRepository.FindBy(x => x.User == user && x.Active == true);

                if (userShoppingCart == null)
                {
                    throw new BllException(ShopExceptionMassages.ShoppingCartExceptionMassages.NOT_FOUNG);
                }
                return userShoppingCart.MapToViewModel();
            }
        }

        public void AddProductToShoppingCart(Guid userId, Guid productId, int quality)
        {
            // Never trust a client-supplied quantity: it must be a positive, sane value.
            if (quality < 1)
            {
                quality = 1;
            }
            else if (quality > MaxQuantityPerLine)
            {
                quality = MaxQuantityPerLine;
            }

            using (_unitOfWork.BeginTransaction())
            {
                var user = _userRepository.FindBy(userId);
                if (user == null)
                    throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);

                var product = _productRepository.FindBy(productId);
                if (product == null)
                    throw new BllException(ShopExceptionMassages.ProductsExceptionMassages.NOT_FOUND_EXCEPTION);

                ShoppingCart shoppingCart;
                shoppingCart = _shoppingCartRepository.FindBy(x => x.User == user && x.Active == true);
                if (shoppingCart == null)
                {
                    shoppingCart = new ShoppingCart
                    {
                        User = user,
                        Active = true,
                        Name = "Cart#" + Guid.NewGuid().ToString("N").Substring(0, 8)
                    };

                    _shoppingCartRepository.Add(shoppingCart);

                    var shoppingCartDetail = new ShoppingCartDetail
                    {
                        Product = product,
                        ShoppingCart = shoppingCart, // Assign the shopping cart instance
                        Quantity = quality
                    };

                    _shoppingCartDetailRepository.Add(shoppingCartDetail);
                    _unitOfWork.Commit();
                }
                else
                {
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

                        if (shoppingDetail.Quantity < MaxQuantityPerLine)
                        {
                            shoppingDetail.Quantity++;
                        }

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

                EnsureCartIsActive(detailCart);

                _shoppingCartDetailRepository.Delete(detailCart);
                _unitOfWork.Commit();
            }
        }

        public void PlusQuantity(Guid detailId)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var shoppingCardDetail = _shoppingCartDetailRepository.FindBy(detailId)
                    ?? throw new BllException(ShopExceptionMassages.ShoppingCartExceptionMassages.DETAIL_NOT_FOUND_EXCEPTION);

                EnsureCartIsActive(shoppingCardDetail);

                if (shoppingCardDetail.Quantity < MaxQuantityPerLine)
                {
                    shoppingCardDetail.Quantity++;
                }

                _shoppingCartDetailRepository.Update(shoppingCardDetail);

                _unitOfWork.Commit();
            }
        }

        public void MinusQuantity(Guid detailId)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var shoppingCardDetail = _shoppingCartDetailRepository.FindBy(detailId)
                    ?? throw new BllException(ShopExceptionMassages.ShoppingCartExceptionMassages.DETAIL_NOT_FOUND_EXCEPTION);

                EnsureCartIsActive(shoppingCardDetail);

                // Never let the quantity drop to zero or below (previously this could underflow).
                if (shoppingCardDetail.Quantity > 1)
                {
                    shoppingCardDetail.Quantity--;
                }

                _shoppingCartDetailRepository.Update(shoppingCardDetail);

                _unitOfWork.Commit();
            }
        }

        private static void EnsureCartIsActive(ShoppingCartDetail detail)
        {
            if (detail.ShoppingCart == null || !detail.ShoppingCart.Active)
            {
                throw new BllException(ShopExceptionMassages.ShoppingCartExceptionMassages.NOT_ACTIVE_EXCEPTION);
            }
        }

        /// <summary>True when the shopping cart exists and belongs to the given user.</summary>
        public bool DoesCartBelongToUser(Guid shoppingCartId, Guid userId)
        {
            using (_unitOfWork.BeginTransaction())
            {
                return _shoppingCartRepository.CheckExist(x => x.Id == shoppingCartId && x.User.Id == userId);
            }
        }

        /// <summary>True when the cart detail exists and belongs to the given user's cart.</summary>
        public bool DoesDetailBelongToUser(Guid shoppingCartDetailId, Guid userId)
        {
            using (_unitOfWork.BeginTransaction())
            {
                return _shoppingCartDetailRepository.CheckExist(x =>
                    x.Id == shoppingCartDetailId && x.ShoppingCart.User.Id == userId);
            }
        }

        private const int MaxQuantityPerLine = 999;
    }
}
