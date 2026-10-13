using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace IntellaQuest.Web.Infrastructure
{
    /// <summary>
    /// Returns the same JSON error envelope the API controllers use for unhandled exceptions,
    /// including failures that happen before an action runs (for example dependency-resolution
    /// or database connection errors). Non-API requests are rethrown so the normal error
    /// pipeline handles them.
    /// </summary>
    public sealed class ApiExceptionMiddleware
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        };

        private readonly RequestDelegate _next;

        public ApiExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception) when (IsApiRequest(context))
            {
                if (context.Response.HasStarted)
                {
                    throw;
                }

                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                context.Response.ContentType = "application/json; charset=utf-8";

                var payload = JsonSerializer.Serialize(
                    new { success = false, exception = exception.Message },
                    SerializerOptions);

                await context.Response.WriteAsync(payload);
            }
        }

        private static bool IsApiRequest(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
        }
    }
}
