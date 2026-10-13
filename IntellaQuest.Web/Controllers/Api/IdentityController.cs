using System;
using IntellaQeust.BusinessLogic.ViewModels;
using IntellaQuest.BusinessLogic.Services;
using IntellaQuest.BusinessLogic.ViewModels;
using IntellaQuest.Web.Controllers.Api;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult ChangePassword(ChangePasswordUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelState.Values);
            }

            _userService.UpdatePassword(model);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult GetUserDetails(Guid id)
        {
            return Json(_userService.Get(id));
        }

        [HttpPost]
        public IActionResult SetUserDetails(UserViewModel model)
        {
            _userService.Update(model);
            return Json(true);
        }

        [HttpPost]
        public IActionResult UpdatePassword(ChangePasswordUserViewModel model)
        {
            _userService.UpdatePassword(model);
            return Json(true);
        }

        [HttpPost]
        public IActionResult GetAmountMoneyOfUser(Guid userId)
        {
            var amount = _userService.GetAmountMoneyOfUser(userId);
            return Json(amount);
        }

        [HttpPost]
        public IActionResult AddPayment(Guid userId, PaymentsViewModelInfo model)
        {
            _userService.AddPayment(userId, model);
            return Json(true);
        }

        [HttpPost]
        public IActionResult DeletePayment(Guid userId)
        {
            _userService.DeletePayment(userId);
            return Json(true);
        }

        #endregion
    }
}
