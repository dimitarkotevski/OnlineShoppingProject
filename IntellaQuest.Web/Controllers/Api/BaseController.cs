using System;
using System.Linq;
using System.Net;
using IntellaQuest.BusinessLogic.Exceptions;
using IntellaQuest.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IntellaQuest.Web.Controllers.Api
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

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
            context.Result = new JsonResult(new { success = false, exception = message });
            context.ExceptionHandled = true;
        }

        [NonAction]
        public IActionResult ErrorNotAcceptable()
        {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors
                    .Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? e.Exception?.Message : e.ErrorMessage)
                    .ToArray());

            return new JsonResult(new { Success = false, Errors = errors });
        }
    }
}
