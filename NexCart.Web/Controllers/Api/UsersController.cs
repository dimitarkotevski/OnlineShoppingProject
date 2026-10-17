using System;
using NexCart.Business.Requests;
using NexCart.Business.Services;
using NexCart.Business.ViewModels;
using NexCart.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NexCart.Web.Controllers
{
    [Route("api/[controller]/[action]/{id?}")]
    [Authorize(Policy = "AdminOnly")]
    public class UsersController : BaseController
    {
        private readonly IUserService _usersService;

        public UsersController(IUserService usersService)
        {
            _usersService = usersService;
        }

        [HttpPost]
        public IActionResult All(RequestModel request)
        {
            return Json(_usersService.GetAll(request));
        }

        [HttpPost]
        public IActionResult Get(Guid id)
        {
            return Json(_usersService.Get(id));
        }

        [HttpPost]
        public IActionResult Create(UserViewModel model)
        {
            _usersService.Create(model);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult Update(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelState.Values);
            }

            _usersService.Update(model);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult Delete(Guid id)
        {
            _usersService.Delete(id);
            return Json(new { success = true });
        }
    }
}
