using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace WebApplication.Filters
{
    public class ValidateModelStateFilter : ActionFilterAttribute, IExceptionFilter
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.ModelState.IsValid)
            {
                throw new ValidationException();
            }
        }

        public void OnException(ExceptionContext context)
        {
            context.Result = new RedirectToRouteResult(new RouteValueDictionary
            {
                {"controller", "Error"},
                {"action", "BadRequestPage"}
            });
        }
    }
}