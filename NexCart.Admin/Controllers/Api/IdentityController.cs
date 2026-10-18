using NexCart.Business.Services;
using NexCart.Business.ViewModels;
using NexCart.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace NexCart.Admin.Controllers
{
    [Route("api/[controller]/[action]/{id?}")]
    public class IdentityController : BaseController
    {
        private readonly IUserService _userService;

        public IdentityController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>Sign-in for the admin site; only administrator accounts are accepted.</summary>
        [HttpPost]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public IActionResult AdminLogin(UserLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return ErrorNotAcceptable();
            }

            var userToken = _userService.Login(model, LoginPortal.Admin);
            return Ok(userToken);
        }
    }
}
