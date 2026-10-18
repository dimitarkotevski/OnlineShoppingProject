using System;
using System.Collections.Generic;
using System.Linq;
using NexCart.Business.Requests;
using NexCart.Business.Services;
using NexCart.Business.ViewModels;
using NexCart.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NexCart.Admin.Controllers
{
    [Route("api/[controller]/[action]/{id?}")]
    [Authorize(Policy = "AdminOnly")]
    public class OrdersController : BaseController
    {
        private const int MaxBulkDelete = 500;

        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public IActionResult All(RequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return ErrorNotAcceptable();
            }

            return Ok(_orderService.GetAll(request));
        }

        [HttpPost]
        public IActionResult Get(Guid Id)
        {
            return Ok(_orderService.Get(Id));
        }

        [HttpPost]
        public IActionResult Create(OrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return ErrorNotAcceptable();
            }

            _orderService.Create(model);
            return Ok(new { success = true });
        }

        [HttpPost]
        public IActionResult Edit(OrderViewModel model)
        {
            _orderService.Update(model);
            return Ok(new { success = true });
        }

        [HttpPost]
        public IActionResult Delete(Guid Id)
        {
            _orderService.Delete(Id);
            return Ok(new { success = true });
        }

        /// <summary>Deletes the selected orders in one go (all or nothing).</summary>
        [HttpPost]
        public IActionResult DeleteMany(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                ModelState.AddModelError("Ids", "Select at least one order.");
                return ErrorNotAcceptable();
            }

            if (ids.Count > MaxBulkDelete)
            {
                ModelState.AddModelError("Ids", $"You can delete at most {MaxBulkDelete} orders at once.");
                return ErrorNotAcceptable();
            }

            _orderService.DeleteMany(ids);
            return Ok(new { success = true, deleted = ids.Distinct().Count() });
        }
    }
}
