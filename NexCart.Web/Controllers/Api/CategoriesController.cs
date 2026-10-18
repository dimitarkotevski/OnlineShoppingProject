using System;
using NexCart.Business.Requests;
using NexCart.Business.Services;
using NexCart.Web.Controllers.Api;
using Microsoft.AspNetCore.Mvc;

namespace NexCart.Web.Controllers
{
    [Route("api/[controller]/[action]/{id?}")]
    public class CategoriesController : BaseController
    {
        private readonly ICategoryService _categoriesService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoriesService = categoryService;
        }

        [HttpPost]
        public IActionResult AllCategories()
        {
            // Categories switched off in the admin tool are hidden from the storefront.
            var data = _categoriesService.GetAll(activeOnly: true);
            var count = data.Count;
            return Ok(new { Items = data, TotalItems = count });
        }

        [HttpPost]
        public IActionResult GetProductsByCategory(Guid id, RequestModel request)
        {
            var gridProducts = _categoriesService.GetProductsByCategory(id, request);
            return Ok(gridProducts);
        }

        [HttpPost]
        public IActionResult GetCategoryById(Guid id)
        {
            var gridProducts = _categoriesService.GetCategoryById(id);
            return Ok(gridProducts);
        }
    }
}
