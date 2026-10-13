using System;
using IntellaQeust.BusinessLogic.Requests;
using IntellaQeust.BusinessLogic.ViewModels;
using IntellaQuest.BusinessLogic.Services;
using IntellaQuest.Web.Controllers.Api;
using Microsoft.AspNetCore.Mvc;

namespace IntellaQuest.Web.Controllers
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
            return Json(_productsService.GetAll(request));
        }

        [HttpPost]
        public IActionResult GetTable()
        {
            var data = _productsService.GetAllTable();
            var recordsTotal = data.Count;
            return Json(new { data, recordsTotal });
        }

        [HttpPost]
        public IActionResult Get(Guid Id)
        {
            return Json(_productsService.Get(Id));
        }

        [HttpPost]
        public IActionResult GetProductImage(Guid id)
        {
            var image = _productsService.GetProductImage(id);
            return Json(image);
        }

        #region ADMIN ACTION

        [HttpPost]
        public IActionResult Create(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelState.Values);
            }

            _productsService.Create(model);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult Edit(ProductViewModel model)
        {
            _productsService.Update(model);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult Delete(Guid Id)
        {
            return Json(new { success = _productsService.Delete(Id) });
        }

        [HttpPost]
        public IActionResult AddImageToProduct(Guid id, string image)
        {
            _productsService.UploadImage(id, image);
            return Json(true);
        }

        [HttpPost]
        public IActionResult GetAllBrands(Guid categoryId)
        {
            return Json(_productsService.GetAllBrands(categoryId));
        }

        #endregion
    }
}
