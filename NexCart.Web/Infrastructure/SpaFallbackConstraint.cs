using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace NexCart.Web.Infrastructure
{
    /// <summary>
    /// Route constraint for the SPA fallback: matches paths that are not files and are not API
    /// requests, so an unknown /api/... URL returns 404 instead of the Angular shell.
    /// </summary>
    public sealed class SpaFallbackConstraint : IRouteConstraint
    {
        public bool Match(
            HttpContext httpContext,
            IRouter route,
            string parameterName,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            if (!values.TryGetValue(parameterName, out var value) || !(value is string path) || string.IsNullOrEmpty(path))
            {
                return true;
            }

            if (IsUnderSegment(path, "api"))
            {
                return false;
            }

            // /admin/... is redirected to the separate admin site (NexCart.Admin), not the shop shell.
            if (IsUnderSegment(path, "admin"))
            {
                return false;
            }

            // Mirror the built-in "nonfile" constraint: a dot in the final segment means a file.
            var trimmed = path.TrimEnd('/');
            var lastSlash = trimmed.LastIndexOf('/');
            var lastSegment = lastSlash >= 0 ? trimmed.Substring(lastSlash + 1) : trimmed;

            return !lastSegment.Contains('.');
        }

        /// <summary>True for "segment" itself or anything below it ("segment/..."), ignoring case.</summary>
        private static bool IsUnderSegment(string path, string segment)
        {
            return string.Equals(path, segment, StringComparison.OrdinalIgnoreCase)
                || path.StartsWith(segment + "/", StringComparison.OrdinalIgnoreCase);
        }
    }
}
