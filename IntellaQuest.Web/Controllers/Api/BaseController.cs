using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IntellaQuest.Web.Controllers.Api
{
    [ApiController]
    public class BaseController : Controller, IExceptionFilter
    {
        /// <summary>
        /// Mirrors the old MVC 5 OnException override: unhandled exceptions become a
        /// 409 Conflict carrying { success: false, exception: "..." }.
        /// </summary>
        [NonAction]
        public void OnException(ExceptionContext context)
        {
            if (context.ExceptionHandled)
            {
                return;
            }

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
            context.Result = new JsonResult(new { success = false, exception = context.Exception.Message });
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
