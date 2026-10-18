using NexCart.Business.Services;
using NexCart.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NexCart.Admin.Controllers
{
    [Route("api/[controller]/[action]/{id?}")]
    [Authorize(Policy = "AdminOnly")]
    public class CategoriesController : BaseController
    {
        private readonly ICategoryService _categoriesService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoriesService = categoryService;
        }

        /// <summary>Categories for the product form's category select.</summary>
        [HttpPost]
        public IActionResult AllCategories()
        {
            var data = _categoriesService.GetAll();
            var count = data.Count;
            return Ok(new { Items = data, TotalItems = count });
        }
    }
}
