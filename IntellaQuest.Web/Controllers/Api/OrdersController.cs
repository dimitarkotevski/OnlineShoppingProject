using System;
using IntellaQuest.BusinessLogic.Requests;
using IntellaQuest.BusinessLogic.Services;
using IntellaQuest.BusinessLogic.ViewModels;
using IntellaQuest.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntellaQuest.Web.Controllers
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

        #region ADMIN ACTION

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult All(RequestModel request)
        {
            return Json(_orderService.GetAll(request));
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Get(Guid Id)
        {
            return Json(_orderService.Get(Id));
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Create(OrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelState.Values);
            }

            _orderService.Create(model);
            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Edit(OrderViewModel model)
        {
            _orderService.Update(model);
            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Delete(Guid Id)
        {
            _orderService.Delete(Id);
            return Json(new { success = true });
        }

        #endregion

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
            return Json(result);
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
            return Json(result);
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
            return Json(true);
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
            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize]
        public IActionResult MakeAPay(OrderViewModel model)
        {
            if (!IsAdmin && !_orderService.DoesOrderBelongToUser(model.Id, CurrentUserId))
            {
                return Forbid();
            }

            _orderService.MakeAPay(model);
            return Json(new { success = true });
        }

        #endregion
    }
}
