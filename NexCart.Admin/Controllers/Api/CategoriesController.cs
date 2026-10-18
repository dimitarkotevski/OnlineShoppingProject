using System;
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

        /// <summary>Paged, filtered rows for the categories grid.</summary>
        [HttpPost]
        public IActionResult All(RequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return ErrorNotAcceptable();
            }

            return Ok(_categoriesService.GetTable(request ?? new RequestModel()));
        }

        [HttpPost]
        public IActionResult Create(CategoryViewModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return ErrorNotAcceptable();
            }

            _categoriesService.Create(model);
            return Ok(new { success = true });
        }

        [HttpPost]
        public IActionResult Edit(CategoryViewModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return ErrorNotAcceptable();
            }

            _categoriesService.Update(model);
            return Ok(new { success = true });
        }

        [HttpPost]
        public IActionResult Delete(Guid id)
        {
            _categoriesService.Delete(id);
            return Ok(new { success = true });
        }
    }
}
