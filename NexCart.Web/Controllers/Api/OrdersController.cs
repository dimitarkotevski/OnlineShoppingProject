using System;
using NexCart.Business.Services;
using NexCart.Business.ViewModels;
using NexCart.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NexCart.Web.Controllers
{
    [Route("api/[controller]/[action]/{id?}")]
    public class OrdersController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly IShoppingCartService _shoppingCartService;

        public OrdersController(IOrderService orderService, IShoppingCartService shoppingCartService)
        {
            _orderService = orderService;
            _shoppingCartService = shoppingCartService;
        }

        #region USER ACTION

        [HttpPost]
        [Authorize]
        public IActionResult GetUserActiveOrder(Guid userId)
        {
            if (!IsAdmin)
            {
                userId = CurrentUserId;
            }

            var result = _orderService.GetUserActiveOrder(userId);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public IActionResult GetUserNotActiveOrders(Guid userId)
        {
            if (!IsAdmin)
            {
                userId = CurrentUserId;
            }

            var result = _orderService.GetUserNotActiveOrders(userId);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public IActionResult CancelActiveOrder(Guid orderId)
        {
            if (!IsAdmin && !_orderService.DoesOrderBelongToUser(orderId, CurrentUserId))
            {
                return Forbid();
            }

            _orderService.CancelActiveOrder(orderId);
            return Ok(true);
        }

        [HttpPost]
        [Authorize]
        public IActionResult MakeAnOrder(Guid shoppingCartId, Guid userId)
        {
            if (!IsAdmin)
            {
                userId = CurrentUserId;
            }

            if (!_shoppingCartService.DoesCartBelongToUser(shoppingCartId, userId))
            {
                return Forbid();
            }

            _orderService.MakeAnOrder(shoppingCartId, userId);
            return Ok(new { success = true });
        }

        [HttpPost]
        [Authorize]
        public IActionResult MakeAPay(OrderViewModel model)
        {
            if (!IsAdmin && !_orderService.DoesOrderBelongToUser(model.Id, CurrentUserId))
            {
                return Forbid();
            }

            var orderStatus = _orderService.MakeAPay(model);
            return Ok(new { success = true, orderStatus = (int)orderStatus });
        }

        #endregion
    }
}
