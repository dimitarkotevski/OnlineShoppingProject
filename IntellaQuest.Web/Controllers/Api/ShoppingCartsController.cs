using System;
using IntellaQuest.BusinessLogic.Services;
using IntellaQuest.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntellaQuest.Web.Controllers
{
    [Route("api/[controller]/[action]/{id?}")]
    [Authorize]
    public class ShoppingCartsController : BaseController
    {
        private readonly IShoppingCartService _shoppingCartService;

        public ShoppingCartsController(IShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        #region User Action

        [HttpPost]
        public IActionResult GetUserShoppingCartWithProducts(Guid userId)
        {
            if (!IsAdmin)
            {
                userId = CurrentUserId;
            }

            return Json(_shoppingCartService.GetUserShoppingCartWithProducts(userId));
        }

        [HttpPost]
        public IActionResult AddProductToShoppingCart(Guid userId, Guid productId, int quality = 1)
        {
            if (!IsAdmin)
            {
                userId = CurrentUserId;
            }

            _shoppingCartService.AddProductToShoppingCart(userId, productId, quality);
            return Json(true);
        }

        [HttpPost]
        public IActionResult RemoveProductFromShoppingCart(Guid detailsId)
        {
            if (!IsAdmin && !_shoppingCartService.DoesDetailBelongToUser(detailsId, CurrentUserId))
            {
                return Forbid();
            }

            _shoppingCartService.RemoveProductFromShoppingCart(detailsId);
            return Json(true);
        }

        [HttpPost]
        public IActionResult MinusQuantity(Guid detailsId)
        {
            if (!IsAdmin && !_shoppingCartService.DoesDetailBelongToUser(detailsId, CurrentUserId))
            {
                return Forbid();
            }

            _shoppingCartService.MinusQuantity(detailsId);
            return Json(true);
        }

        [HttpPost]
        public IActionResult PlusQuantity(Guid detailsId)
        {
            if (!IsAdmin && !_shoppingCartService.DoesDetailBelongToUser(detailsId, CurrentUserId))
            {
                return Forbid();
            }

            _shoppingCartService.PlusQuantity(detailsId);
            return Json(true);
        }

        #endregion
    }
}
