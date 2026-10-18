using System;
using NexCart.Business.Requests;
using NexCart.Business.Services;
using NexCart.Web.Controllers.Api;
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

        [HttpPost]
        public IActionResult GetAllBrands(Guid categoryId)
        {
            return Ok(_productsService.GetAllBrands(categoryId));
        }
    }
}
