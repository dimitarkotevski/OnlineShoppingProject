using System;
using NexCart.Business.Services;
using NexCart.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NexCart.Web.Controllers
{
    [Route("api/[controller]/[action]/{id?}")]
    [Authorize]
    public class FavouriteProductsController : BaseController
    {
        private readonly IFavouriteProductService _favouriteProductService;

        public FavouriteProductsController(IFavouriteProductService favouriteProductService)
        {
            _favouriteProductService = favouriteProductService;
        }

        #region USER ACTION

        [HttpPost]
        public IActionResult UserAddFavouriteProduct(Guid userId, Guid productId)
        {
            if (!IsAdmin)
            {
                userId = CurrentUserId;
            }

            _favouriteProductService.AddProductToFavouriteProduct(userId, productId);
            return Ok(true);
        }

        [HttpPost]
        public IActionResult GetUserFavouriteProducts(Guid userId)
        {
            if (!IsAdmin)
            {
                userId = CurrentUserId;
            }

            var gridProducts = _favouriteProductService.GetUserFavouriteProducts(userId);
            return Ok(gridProducts);
        }

        [HttpPost]
        public IActionResult RemoveFavouriteProduct(Guid userId, Guid productId)
        {
            if (!IsAdmin)
            {
                userId = CurrentUserId;
            }

            _favouriteProductService.RemoveProductFromFavuriteProduct(userId, productId);
            return Ok(true);
        }

        #endregion
    }
}
