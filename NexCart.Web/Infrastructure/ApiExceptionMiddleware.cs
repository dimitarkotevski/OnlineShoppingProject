using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using NexCart.Business.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace NexCart.Web.Infrastructure
{
    /// <summary>
    /// Returns the same JSON error envelope the API controllers use for unhandled exceptions,
    /// including failures that happen before an action runs (for example dependency-resolution
    /// or database connection errors). Non-API requests are rethrown so the normal error
    /// pipeline handles them. Business exceptions keep their message; everything else is masked.
    /// </summary>
    public sealed class ApiExceptionMiddleware
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        };

        private readonly RequestDelegate _next;
        private readonly ILogger<ApiExceptionMiddleware> _logger;

        public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
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

                if (exception is BllException)
                {
                    _logger.LogWarning(exception, "Business rule violation on {Path}", context.Request.Path);
                }
                else
                {
                    // Log the detail server-side; never expose it to the caller.
                    _logger.LogError(exception, "Unhandled exception on {Path}", context.Request.Path);
                }

                var message = exception is BllException
                    ? exception.Message
                    : "An unexpected error occurred.";

                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                context.Response.ContentType = "application/json; charset=utf-8";

                var payload = JsonSerializer.Serialize(
                    new { success = false, exception = message },
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
