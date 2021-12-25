using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BadNews.Elevation
{
    public class ElevationRequiredFilterAttribute : Attribute, IResourceFilter
    {
        // Этот метод вызовется еще перед вызовом метода контроллера
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (!context.HttpContext.Request.IsElevated())
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        // Этот метод вызовется уже после вызова метода контроллера
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }
    }
}
