using System;
using NexCart.Business.Requests;
using NexCart.Business.ViewModels;
using NexCart.Business.Services;
using NexCart.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NexCart.Web.Controllers
{
    [Route("api/[controller]/[action]/{id?}")]
    public class ProductsController : BaseController
    {
        private readonly IProductService _productsService;

        public ProductsController(IProductService productsService)
        {
            _productsService = productsService;
        }

        [HttpPost]
        public IActionResult All(RequestModel request)
        {
            return Ok(_productsService.GetAll(request));
        }

        [HttpPost]
        public IActionResult Get(Guid Id)
        {
            return Ok(_productsService.Get(Id));
        }

        #region ADMIN ACTION

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
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
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Edit(ProductViewModel model)
        {
            _productsService.Update(model);
            return Ok(new { success = true });
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Delete(Guid Id)
        {
            return Ok(new { success = _productsService.Delete(Id) });
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult AddImageToProduct(Guid id, string image)
        {
            _productsService.UploadImage(id, image);
            return Ok(true);
        }

        [HttpPost]
        public IActionResult GetAllBrands(Guid categoryId)
        {
            return Ok(_productsService.GetAllBrands(categoryId));
        }

        #endregion
    }
}
