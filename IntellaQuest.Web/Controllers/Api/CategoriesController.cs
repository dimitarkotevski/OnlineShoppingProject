using System;
using IntellaQeust.BusinessLogic.Requests;
using IntellaQeust.BusinessLogic.ViewModels;
using IntellaQuest.BusinessLogic.Services;
using IntellaQuest.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntellaQuest.Web.Controllers
{
    [Route("api/[controller]/[action]/{id?}")]
    public class CategoriesController : BaseController
    {
        private readonly ICategoryService _categoriesService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoriesService = categoryService;
        }

        #region ADMIN ACTION

        [HttpPost]
        public IActionResult AllCategories()
        {
            var data = _categoriesService.GetAll();
            var count = data.Count;
            return Json(new { Items = data, TotalItems = count });
        }

        [HttpPost]
        public IActionResult GetProductsByCategory(Guid id, RequestModel request)
        {
            var gridProducts = _categoriesService.GetProductsByCategory(id, request);
            return Json(gridProducts);
        }

        [HttpPost]
        public IActionResult GetCategoryById(Guid id)
        {
            var gridProducts = _categoriesService.GetCategoryById(id);
            return Json(gridProducts);
        }

        [HttpPost]
        public IActionResult All(RequestModel request)
        {
            return Json(_categoriesService.GetAll(request));
        }

        [HttpPost]
        public IActionResult Get(Guid id)
        {
            return Json(_categoriesService.Get(id));
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return ErrorNotAcceptable();
            }

            _categoriesService.Create(model);
            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Edit(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return ErrorNotAcceptable();
            }

            _categoriesService.Update(model);
            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult DeleteById(Guid id)
        {
            _categoriesService.DeleteById(id);
            return Json(new { success = true });
        }

        #endregion
    }
}
