using CodegenCS;
using Serilog;

namespace Generator.Core
{
    public static class CustomExceptionGenerator
    {
        private static readonly string cl = $"CustomException";

        public static (ICodegenOutputFile, string?) Generator(string tempFilePath)
        {
            Log.Debug("Adding ~ {Class}", cl);

            var ctx = new CodegenContext();
            var w = ctx[$"{cl}.cs"];

            w.WriteLine($$"""
            namespace Utils
            {
                public class CustomException : Exception
                {
                    public int StatusCode { get; set; }
                    public string CustomCode { get; set; }

                    public CustomException(string message, int statusCode, string customCode = "") : base(message)
                    {
                        StatusCode = statusCode;
                        CustomCode = customCode;
                    }
                }
            }
            """);

            return (w, $"{tempFilePath}/src/Domain/Utils/");

        }
    }
}
