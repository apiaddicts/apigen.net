using Api.Models.Out;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System.Net;

namespace Api.Helpers
{
    public class HttpResponseExceptionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {

            if (context.Exception is Exception ex)
            {
                HttpStatusCode status = HttpStatusCode.InternalServerError;

                if (ex is HttpRequestException exh)
                    status = exh.StatusCode ?? status;

                Log.Error(ex.Message);
                var errors = new List<ErrorResponse>() { new() { Message = ex.Message, Code = status } };
                context.Result = new ObjectResult(new { errors }) { StatusCode = (int)errors.First().Code };
                context.ExceptionHandled = true;
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => new { code = 400, message = e.ErrorMessage })
                    .ToList();

                Log.Warning(string.Join(", ", errors.Select(e => e.message)));
                context.Result = new ObjectResult(new { errors }) { StatusCode = 400 };
            }
        }
    }
}
