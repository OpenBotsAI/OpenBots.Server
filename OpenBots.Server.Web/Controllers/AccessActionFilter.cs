using OpenBots.Server.DataAccess.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;

namespace OpenBots.Server.WebAPI.Controllers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AccessActionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var isAuthAccessError = context.Exception is UnauthorizedAccessException;
            var isAuthOperationError = context.Exception is UnauthorizedOperationException;

            if (!isAuthAccessError && !isAuthOperationError)
            {
                var result = ExceptionExtentions.GetActionResult(context.Exception) as ObjectResult;
                var errorContext = result.Value;
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = 500;
                context.Result = new JsonResult(new
                {
                    error = errorContext 
                });
            }
            else
            {
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = 403;
                context.Result = new JsonResult(new
                {
                    error = "You do not have access or entity does not exist.",
                    statusCode = HttpStatusCode.Forbidden
                });
            }
        }
    }
}

