using System;
using System.Linq;
using System.Security.Claims;
using NexCart.Domain;
using NexCart.Domain.Enums;

namespace NexCart.Web.Infrastructure
{
    /// <summary>
    /// Helpers for reading the identity of the caller from a JWT. The lookups accept both the
    /// long .NET claim types and the short JWT names ("nameid", "sub", "role"), so the result
    /// does not depend on whether inbound claim-type mapping is enabled.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        private static readonly string[] UserIdClaimTypes =
        {
            ClaimTypes.NameIdentifier,
            "nameid",
            "sub"
        };

        private static readonly string[] RoleClaimTypes =
        {
            ClaimTypes.Role,
            "role",
            "roles"
        };

        public static Guid GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                return Guid.Empty;
            }

            foreach (var type in UserIdClaimTypes)
            {
                var value = principal.FindFirst(type)?.Value;
                if (Guid.TryParse(value, out var id))
                {
                    return id;
                }
            }

            return Guid.Empty;
        }

        /// <summary>The role named in the token, or <c>null</c> when there is no role claim.</summary>
        public static string GetRole(this ClaimsPrincipal principal)
        {
            return principal?.Claims
                .FirstOrDefault(c => RoleClaimTypes.Contains(c.Type, StringComparer.OrdinalIgnoreCase))
                ?.Value;
        }

        public static bool IsAdministrator(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                return false;
            }

            return principal.Claims.Any(c =>
                RoleClaimTypes.Contains(c.Type, StringComparer.OrdinalIgnoreCase) &&
                string.Equals(c.Value, UserRole.Admin.ToString(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
