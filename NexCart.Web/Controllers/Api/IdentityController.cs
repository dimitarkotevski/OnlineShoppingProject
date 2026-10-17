using System;
using NexCart.Business.ViewModels;
using NexCart.Business.Services;
using NexCart.Business.ViewModels;
using NexCart.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace NexCart.Web.Controllers
{
    [Route("api/[controller]/[action]/{id?}")]
    public class IdentityController : BaseController
    {
        private readonly IUserService _userService;

        public IdentityController(IUserService userService)
        {
            _userService = userService;
        }

        #region USER AUTHENTIFICATION

        [HttpPost]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public IActionResult Registration(UserRegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return ErrorNotAcceptable();
            }

            _userService.Registration(model);
            return Ok(true);
        }

        [HttpPost]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public IActionResult Login(UserLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return ErrorNotAcceptable();
            }

            var userToken = _userService.Login(model);
            return Ok(userToken);
        }

        #endregion

        #region USER ACTION

        [HttpPost]
        [Authorize]
        [EnableRateLimiting("auth")]
        public IActionResult ChangePassword(ChangePasswordUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return ErrorNotAcceptable();
            }

            // The target user is always the caller; a client-supplied id is ignored.
            model.Id = CurrentUserId;
            _userService.UpdatePassword(model);
            return Ok(new { success = true });
        }

        [HttpPost]
        [Authorize]
        public IActionResult GetUserDetails(Guid id)
        {
            if (!IsSelfOrAdmin(id))
            {
                return Forbid();
            }

            return Ok(_userService.Get(id));
        }

        [HttpPost]
        [Authorize]
        public IActionResult SetUserDetails(UserViewModel model)
        {
            // Users may only edit their own profile, and never their own role or password.
            model.Id = CurrentUserId;
            model.Role = null;
            model.Password = null;

            _userService.Update(model);
            return Ok(true);
        }

        [HttpPost]
        [Authorize]
        public IActionResult GetAmountMoneyOfUser(Guid userId)
        {
            if (!IsAdmin)
            {
                userId = CurrentUserId;
            }

            var amount = _userService.GetAmountMoneyOfUser(userId);
            return Ok(amount);
        }

        [HttpPost]
        [Authorize]
        public IActionResult AddPayment(Guid userId, PaymentsViewModelInfo model)
        {
            if (!IsAdmin)
            {
                userId = CurrentUserId;
            }

            _userService.AddPayment(userId, model);
            return Ok(true);
        }

        [HttpPost]
        [Authorize]
        public IActionResult DeletePayment(Guid userId)
        {
            if (!IsAdmin)
            {
                userId = CurrentUserId;
            }

            _userService.DeletePayment(userId);
            return Ok(true);
        }

        #endregion
    }
}
