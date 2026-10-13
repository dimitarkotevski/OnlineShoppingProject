using System;
using System.Linq;
using System.Security.Claims;

namespace IntellaQuest.Web.Infrastructure
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

        public static bool IsAdministrator(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                return false;
            }

            return principal.Claims.Any(c =>
                RoleClaimTypes.Contains(c.Type, StringComparer.OrdinalIgnoreCase) &&
                string.Equals(c.Value, "admin", StringComparison.OrdinalIgnoreCase));
        }
    }
}
