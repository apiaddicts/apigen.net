using CodegenCS;
using Microsoft.OpenApi.Models;
using Serilog;

namespace Generator.Core
{
    public static class RequestContextLoggingMiddlewareGenerator
    {
        private readonly static string cl = $"RequestContextLoggingMiddleware";

        public static (ICodegenOutputFile, string?) Generator(string tempFilePath)
        {
            Log.Debug("Adding ~ {Class}", cl);

            var ctx = new CodegenContext();
            var w = ctx[$"{cl}.cs"];

            w.WriteLine($$"""
            using Microsoft.Extensions.Primitives;
            using Serilog;
            using Serilog.Context;

            namespace Helpers
            {
                public class RequestContextLoggingMiddleware(RequestDelegate next)
                {
                    private const string CorrelationIdHeaderName = "X-Trace-Id";
                    private readonly RequestDelegate _next = next;

                    public Task Invoke(HttpContext context)
                    {
                        string correlationId = GetCorrelationId(context);

                        using (LogContext.PushProperty("CorrelationId", correlationId))
                        {
                            context.Response.Headers.Append(CorrelationIdHeaderName, new StringValues(correlationId));
                            return _next.Invoke(context);
                        }
                    }

                    private static string GetCorrelationId(HttpContext context)
                    {
                        context.Request.Headers.TryGetValue(
                            CorrelationIdHeaderName, out StringValues correlationId);

                        var newGuid = Guid.NewGuid().ToString();
                        Log.Debug($"({newGuid}) [{context.TraceIdentifier}]");
                        return correlationId.FirstOrDefault() ?? newGuid;
                    }
                }
            }
            """);

            return (w, $"{tempFilePath}/src/Api/Helpers/");
        }
    }
}
