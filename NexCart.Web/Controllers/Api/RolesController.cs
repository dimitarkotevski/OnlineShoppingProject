using System;
using System.Linq;
using NexCart.Domain.Enums;
using NexCart.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NexCart.Web.Controllers
{
    [Route("api/[controller]/[action]/{id?}")]
    public class RolesController : BaseController
    {
        /// <summary>
        /// The roles that can be assigned to a user, sourced from the <see cref="UserRole"/> enum.
        /// Kept in the same { Id, Name } shape the admin user form expects, so it stays data-driven
        /// on the client without needing a Roles table.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult All()
        {
            var roles = Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .Select(role => new { Id = (int)role, Name = role.ToString() })
                .OrderBy(x => x.Name)
                .ToList();

            return Json(roles);
        }
    }
}
