using System;
using System.Reflection;
using System.Threading.Tasks;
using FluentValidation;
using NexCart.Business.Validation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NexCart.Web.Infrastructure
{
    /// <summary>
    /// Runs the FluentValidation validator referenced by a [Validator] attribute on an action
    /// argument and adds its failures to ModelState. This mirrors the automatic validation that
    /// FluentValidation.Mvc5 performed in the old ASP.NET MVC 5 application (only types that
    /// carry the attribute are validated).
    /// </summary>
    public sealed class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument == null)
                {
                    continue;
                }

                var attribute = argument.GetType().GetCustomAttribute<FluentValidatorAttribute>();
                if (attribute == null || attribute.ValidatorType == null)
                {
                    continue;
                }

                var validator = context.HttpContext.RequestServices.GetService(attribute.ValidatorType) as IValidator
                    ?? Activator.CreateInstance(attribute.ValidatorType) as IValidator;

                if (validator == null)
                {
                    continue;
                }

                var result = validator.Validate(new ValidationContext<object>(argument));
                foreach (var failure in result.Errors)
                {
                    context.ModelState.AddModelError(failure.PropertyName ?? string.Empty, failure.ErrorMessage);
                }
            }

            await next();
        }
    }
}
