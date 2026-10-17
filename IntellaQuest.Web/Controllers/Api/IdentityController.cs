using System;
using IntellaQuest.BusinessLogic.ViewModels;
using IntellaQuest.BusinessLogic.Services;
using IntellaQuest.BusinessLogic.ViewModels;
using IntellaQuest.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IntellaQuest.Web.Controllers
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
            if (ModelState.IsValid)
            {
                _userService.Registration(model);
                return Json(true);
            }

            return Json(new { success = false });
        }

        [HttpPost]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public IActionResult Login(UserLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelState.Values);
            }

            var userToken = _userService.Login(model);
            return Json(userToken);
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
                return Json(ModelState.Values);
            }

            // The target user is always the caller; a client-supplied id is ignored.
            model.Id = CurrentUserId;
            _userService.UpdatePassword(model);
            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize]
        public IActionResult GetUserDetails(Guid id)
        {
            if (!IsSelfOrAdmin(id))
            {
                return Forbid();
            }

            return Json(_userService.Get(id));
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
            return Json(true);
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
            return Json(amount);
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
            return Json(true);
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
            return Json(true);
        }

        #endregion
    }
}
