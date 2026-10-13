using System;
using IntellaQeust.BusinessLogic.Requests;
using IntellaQeust.BusinessLogic.ViewModels;
using IntellaQuest.BusinessLogic.Services;
using IntellaQuest.Web.Controllers.Api;
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
        public IActionResult GetProductsByCategory(string Url, RequestModel request)
        {
            var gridProducts = _categoriesService.GetProductsByCategory(Url, request);
            return Json(gridProducts);
        }

        [HttpPost]
        public IActionResult GetCategoryByUrl(string Url)
        {
            var gridProducts = _categoriesService.GetCategoryByUrl(Url);
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
        public IActionResult DeleteById(Guid id)
        {
            _categoriesService.DeleteById(id);
            return Json(new { success = true });
        }

        #endregion
    }
}
