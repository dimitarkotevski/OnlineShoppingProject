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
    public class UsersController : BaseController
    {
        private const int MaxBulkDelete = 500;

        private readonly IUserService _usersService;

        public UsersController(IUserService usersService)
        {
            _usersService = usersService;
        }

        [HttpPost]
        public IActionResult All(RequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return ErrorNotAcceptable();
            }

            return Ok(_usersService.GetAll(request));
        }

        [HttpPost]
        public IActionResult Get(Guid id)
        {
            return Ok(_usersService.Get(id));
        }

        [HttpPost]
        public IActionResult Create(UserViewModel model)
        {
            _usersService.Create(model);
            return Ok(new { success = true });
        }

        [HttpPost]
        public IActionResult Update(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return ErrorNotAcceptable();
            }

            _usersService.Update(model);
            return Ok(new { success = true });
        }

        [HttpPost]
        public IActionResult Delete(Guid id)
        {
            _usersService.Delete(id);
            return Ok(new { success = true });
        }

        /// <summary>Deletes the selected users in one go (all or nothing).</summary>
        [HttpPost]
        public IActionResult DeleteMany(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                ModelState.AddModelError("Ids", "Select at least one user.");
                return ErrorNotAcceptable();
            }

            if (ids.Count > MaxBulkDelete)
            {
                ModelState.AddModelError("Ids", $"You can delete at most {MaxBulkDelete} users at once.");
                return ErrorNotAcceptable();
            }

            // Never let an administrator delete their own account (and lock themselves out).
            if (ids.Contains(CurrentUserId))
            {
                ModelState.AddModelError("Ids", "You can't delete your own account.");
                return ErrorNotAcceptable();
            }

            _usersService.DeleteMany(ids);
            return Ok(new { success = true, deleted = ids.Distinct().Count() });
        }
    }
}
