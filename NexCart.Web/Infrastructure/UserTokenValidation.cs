using System;
using System.Threading.Tasks;
using NexCart.Business.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NexCart.Web.Infrastructure
{
    /// <summary>
    /// Re-checks every signature-valid JWT against the database. A token is rejected (401) when
    /// the user it was issued to no longer exists, or when the role it carries no longer matches
    /// the user's current role, so deleting or demoting an account takes effect immediately
    /// instead of when the token expires.
    /// </summary>
    public static class UserTokenValidation
    {
        public static Task OnTokenValidated(TokenValidatedContext context)
        {
            var principal = context.Principal;
            var userId = principal.GetUserId();

            if (userId == Guid.Empty)
            {
                context.Fail("The token does not identify a user.");
                return Task.CompletedTask;
            }

            var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
            var currentRole = userService.GetRole(userId);

            if (currentRole == null)
            {
                Logger(context).LogInformation("Rejected token for user {UserId}: user no longer exists.", userId);
                context.Fail("The user no longer exists.");
                return Task.CompletedTask;
            }

            if (!string.Equals(principal.GetRole(), currentRole.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                Logger(context).LogInformation("Rejected token for user {UserId}: role changed since it was issued.", userId);
                context.Fail("The user's role has changed; sign in again.");
            }

            return Task.CompletedTask;
        }

        private static ILogger Logger(TokenValidatedContext context)
        {
            return context.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(UserTokenValidation));
        }
    }
}
