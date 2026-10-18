using System;
using System.Linq;
using NexCart.Domain.Enums;
using NexCart.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NexCart.Admin.Controllers
{
    /// <summary>An assignable role as exposed to the admin user form ({ Id, Name }).</summary>
    public sealed record RoleModel(int Id, string Name);

    [Route("api/[controller]/[action]/{id?}")]
    [Authorize(Policy = "AdminOnly")]
    public class RolesController : BaseController
    {
        /// <summary>
        /// The roles that can be assigned to a user, sourced from the <see cref="UserRole"/> enum,
        /// in the { Id, Name } shape the admin user form expects.
        /// </summary>
        [HttpPost]
        public IActionResult All()
        {
            var roles = Enum.GetValues<UserRole>()
                .OrderBy(role => role.ToString())
                .Select(role => new RoleModel((int)role, role.ToString()))
                .ToArray();

            return Ok(roles);
        }
    }
}
