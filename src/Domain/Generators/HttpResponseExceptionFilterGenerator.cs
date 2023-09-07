using CodegenCS;
using Microsoft.OpenApi.Models;
using Serilog;

namespace Domain.Generators
{
    public static class HttpResponseExceptionFilterGenerator
    {
        private static string cl = $"HttpResponseExceptionFilter";

        public static (ICodegenOutputFile, string?) Generator(OpenApiDocument doc, string tempFilePath)
        {
            Log.Debug($"Generate {cl}");

            var ctx = new CodegenContext();
            var w = ctx[$"{cl}.cs"];
            w.WriteLine("using Microsoft.AspNetCore.Mvc;");
            w.WriteLine("using Microsoft.AspNetCore.Mvc.Filters;");
            w.WriteLine("using Serilog;\n");
            w.WithCurlyBraces($"namespace Helpers", () =>
            {
                w.WithCurlyBraces($"public class {cl} : IActionFilter", () =>
                {
                    w.WithCurlyBraces($"public void OnActionExecuted(ActionExecutedContext context)", () =>
                    {
                        w.WithCurlyBraces($"if (context.Exception is Exception ex)", () =>
                        {
                            w.WriteLine("Log.Error(ex.Message);");
                            w.WriteLine("var response = new { Messages = new List<string> { ex.Message }, Status = 500 };");
                            w.WriteLine("context.Result = new ObjectResult(response) { StatusCode = response.Status };");
                            w.WriteLine("context.ExceptionHandled = true;");
                        });
                    });

                    w.WithCurlyBraces($"public void OnActionExecuting(ActionExecutingContext context)", () =>
                    {
                        w.WithCurlyBraces($"if (!context.ModelState.IsValid)", () =>
                        {
                            w.WriteLine("var messages = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();");
                            w.WriteLine("var response = new { Messages = messages, Status = 400 };");
                            w.WriteLine("Log.Warning(string.Join(\", \", response.Messages));");
                            w.WriteLine("context.Result = new ObjectResult(response) { StatusCode = response.Status };");
                        });
                    });
                });
            });

            return (w, $"{tempFilePath}/Api/Helpers/");
        }
    }
}
