using System;
using System.Linq;
using System.Text.Json;
using NexCart.Business.Exceptions;
using NexCart.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NexCart.Web.Controllers.Api
{
    [ApiController]
    public class BaseController : Controller, IExceptionFilter
    {
        /// <summary>
        /// Id of the authenticated caller, taken from the JWT (never from the request body).
        /// Returns <see cref="Guid.Empty"/> when there is no authenticated user.
        /// </summary>
        protected Guid CurrentUserId => User.GetUserId();

        /// <summary>True when the caller holds the administrator role.</summary>
        protected bool IsAdmin => User.IsAdministrator();

        /// <summary>True when the caller owns the resource or is an administrator.</summary>
        protected bool IsSelfOrAdmin(Guid ownerId)
        {
            return IsAdmin || (ownerId != Guid.Empty && ownerId == CurrentUserId);
        }

        /// <summary>
        /// Rejects a request whose body could not be deserialized into an action parameter
        /// (recorded by <see cref="JsonBodyModelBinder"/>) before the action runs, so actions
        /// never receive a null model for a malformed payload. Ordinary validation errors are
        /// still left to the actions.
        /// </summary>
        [NonAction]
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var hasMalformedBody = ModelState.Values
                .SelectMany(entry => entry.Errors)
                .Any(error => error.Exception is JsonException);

            if (hasMalformedBody)
            {
                context.Result = ErrorNotAcceptable();
                return;
            }

            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Mirrors the old MVC 5 OnException override: unhandled exceptions become a
        /// 409 Conflict carrying { success: false, exception: "..." }.
        /// Business exceptions keep their message; anything else is masked so internal
        /// details (stack traces, SQL, file paths) never reach the client.
        /// </summary>
        [NonAction]
        public void OnException(ExceptionContext context)
        {
            if (context.ExceptionHandled)
            {
                return;
            }

            var message = context.Exception is BllException
                ? context.Exception.Message
                : "An unexpected error occurred.";

            context.Result = new ConflictObjectResult(new { success = false, exception = message });
            context.ExceptionHandled = true;
        }

        [NonAction]
        public IActionResult ErrorNotAcceptable()
        {
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors
                    .Select(e => e.Exception is JsonException
                        ? "The request body is not in the expected format."
                        : string.IsNullOrEmpty(e.ErrorMessage) ? e.Exception?.Message : e.ErrorMessage)
                    .ToArray());

            return BadRequest(new { Success = false, Errors = errors });
        }
    }
}
