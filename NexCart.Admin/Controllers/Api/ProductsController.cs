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
    public class ProductsController : BaseController
    {
        private const int MaxBulkDelete = 500;

        private readonly IProductService _productsService;

        public ProductsController(IProductService productsService)
        {
            _productsService = productsService;
        }

        [HttpPost]
        public IActionResult All(RequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return ErrorNotAcceptable();
            }

            return Ok(_productsService.GetAll(request));
        }

        [HttpPost]
        public IActionResult Get(Guid Id)
        {
            return Ok(_productsService.Get(Id));
        }

        [HttpPost]
        public IActionResult Create(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return ErrorNotAcceptable();
            }

            _productsService.Create(model);
            return Ok(new { success = true });
        }

        [HttpPost]
        public IActionResult Edit(ProductViewModel model)
        {
            _productsService.Update(model);
            return Ok(new { success = true });
        }

        [HttpPost]
        public IActionResult Delete(Guid Id)
        {
            return Ok(new { success = _productsService.Delete(Id) });
        }

        /// <summary>Deletes the selected products in one go (all or nothing).</summary>
        [HttpPost]
        public IActionResult DeleteMany(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                ModelState.AddModelError("Ids", "Select at least one product.");
                return ErrorNotAcceptable();
            }

            if (ids.Count > MaxBulkDelete)
            {
                ModelState.AddModelError("Ids", $"You can delete at most {MaxBulkDelete} products at once.");
                return ErrorNotAcceptable();
            }

            _productsService.DeleteMany(ids);
            return Ok(new { success = true, deleted = ids.Distinct().Count() });
        }

        [HttpPost]
        public IActionResult AddImageToProduct(Guid id, string image)
        {
            _productsService.UploadImage(id, image);
            return Ok(true);
        }
    }
}
