using CodegenCS;
using Serilog;

namespace Generator.Core
{
    public static class StringUtilsGenerator
    {
        private static readonly string cl = $"StringUtils";

        public static (ICodegenOutputFile, string?) Generator(string tempFilePath)
        {
            Log.Debug("Adding ~ {Class}", cl);

            var ctx = new CodegenContext();
            var w = ctx[$"{cl}.cs"];

            w.WriteLine($$"""
            using System.Linq.Expressions;

            namespace Utils
            {
                public static class StringUtils
                {
                    public static string Capitalize(this string s)
                    {
                        return string.Concat(s[0].ToString().ToUpper(), s.ToLower().AsSpan(1));
                    }
                    public static string Format(this string s)
                    {
                        var split = s.Split(".");
                         List<string> levels = new();
                        foreach (var sp in split)
                        {
                            levels.Add(sp.Capitalize());
                        }
                        return string.Join(".", levels);
                    }
                }
            }
            """);

            return (w, $"{tempFilePath}/src/Domain/Utils/");

        }
    }
}
