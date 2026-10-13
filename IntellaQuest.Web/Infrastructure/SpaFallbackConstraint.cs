using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IntellaQuest.Web.Infrastructure
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

            if (path.StartsWith("api/", StringComparison.OrdinalIgnoreCase)
                || string.Equals(path, "api", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Mirror the built-in "nonfile" constraint: a dot in the final segment means a file.
            var trimmed = path.TrimEnd('/');
            var lastSlash = trimmed.LastIndexOf('/');
            var lastSegment = lastSlash >= 0 ? trimmed.Substring(lastSlash + 1) : trimmed;

            return !lastSegment.Contains('.');
        }
    }
}
