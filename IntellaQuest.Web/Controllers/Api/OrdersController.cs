using System;
using IntellaQeust.BusinessLogic.Requests;
using IntellaQuest.BusinessLogic.Services;
using IntellaQeust.BusinessLogic.ViewModels;
using IntellaQuest.Web.Controllers.Api;
using Microsoft.AspNetCore.Mvc;

namespace IntellaQuest.Web.Controllers
{
    [Route("api/[controller]/[action]/{id?}")]
    public class OrdersController : BaseController
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        #region ADMIN ACTION

        [HttpPost]
        public IActionResult All(RequestModel request)
        {
            return Json(_orderService.GetAll(request));
        }

        [HttpPost]
        public IActionResult Get(Guid Id)
        {
            return Json(_orderService.Get(Id));
        }

        [HttpPost]
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
        public IActionResult Edit(OrderViewModel model)
        {
            _orderService.Update(model);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult Delete(Guid Id)
        {
            _orderService.Delete(Id);
            return Json(new { success = true });
        }

        #endregion

        #region USER ACTION

        [HttpPost]
        public IActionResult GetUserActiveOrder(Guid userId)
        {
            var result = _orderService.GetUserActiveOrder(userId);
            return Json(result);
        }

        [HttpPost]
        public IActionResult GetUserNotActiveOrders(Guid userId)
        {
            var result = _orderService.GetUserNotActiveOrders(userId);
            return Json(result);
        }

        [HttpPost]
        public IActionResult CancelActiveOrder(Guid orderId)
        {
            _orderService.CancelActiveOrder(orderId);
            return Json(true);
        }

        [HttpPost]
        public IActionResult MakeAnOrder(Guid shoppingCartId, Guid userId)
        {
            _orderService.MakeAnOrder(shoppingCartId, userId);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult MakeAPay(OrderViewModel model)
        {
            _orderService.MakeAPay(model);
            return Json(new { success = true });
        }

        #endregion
    }
}
