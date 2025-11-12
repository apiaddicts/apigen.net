using CodegenCS;
using Microsoft.OpenApi.Models;
using Serilog;

namespace Generator.Core
{
    public static class HttpResponseExceptionFilterGenerator
    {
        private readonly static string cl = $"HttpResponseExceptionFilter";

        public static (ICodegenOutputFile, string?) Generator(string tempFilePath)
        {
            Log.Debug("Adding ~ {Class}", cl);

            var ctx = new CodegenContext();
            var w = ctx[$"{cl}.cs"];

            w.WriteLine($$"""
            using Microsoft.AspNetCore.Mvc;
            using Microsoft.AspNetCore.Mvc.Filters;
            using Serilog;
            using Utils;

            namespace Helpers
            {
                public class HttpResponseExceptionFilter : IActionFilter
                {
                    public void OnActionExecuted(ActionExecutedContext context)
                    {
                        if (context.Exception is Exception ex)
                        {
                            Log.Error(ex, ex.Message);
                            int status = 500;

                            if (context.Exception is CustomException ce)
                                status = ce.StatusCode;

                            var response = new { Messages = new List<string> { ex.Message }, Status = status };
                            context.Result = new ObjectResult(response) { StatusCode = response.Status };
                            context.ExceptionHandled = true;
                        }
                    }
                    public void OnActionExecuting(ActionExecutingContext context)
                    {
                        if (!context.ModelState.IsValid)
                        {
                            var messages = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                            var response = new { Messages = messages, Status = 400 };
                            Log.Warning(string.Join(", ", response.Messages));
                            context.Result = new ObjectResult(response) { StatusCode = response.Status };
                        }
                    }

                }
            }
            """);

            return (w, $"{tempFilePath}/src/Api/Helpers/");
        }
    }
}
