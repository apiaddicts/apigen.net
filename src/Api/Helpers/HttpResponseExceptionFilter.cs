using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace Api.Helpers
{
    public class HttpResponseExceptionFilter : IActionFilter
    {

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is Exception ex)
            {
                Log.Error(ex, ex.Message);
                var response = new ErrorResponse { Messages = new List<string> { ex.Message }, Status = 500 };
                context.Result = new ObjectResult(response) { StatusCode = response.Status };
                context.ExceptionHandled = true;
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var messages = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                var response = new ErrorResponse { Messages = messages, Status = 400 };
                Log.Warning(string.Join(", ", response.Messages));
                context.Result = new ObjectResult(response) { StatusCode = response.Status };
            }
        }
    }
}
