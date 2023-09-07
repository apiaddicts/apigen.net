using CodegenCS;
using Serilog;

namespace Domain.Generators
{
    public static class UtilsGenerator
    {
        private static readonly string cl = $"StringUtils";

        public static (ICodegenOutputFile, string?) GeneratorStringUtils(string tempFilePath)
        {
            Log.Debug($"Generate {cl}");

            var ctx = new CodegenContext();
            var w = ctx[$"{cl}.cs"];

            w.WithCurlyBraces($"namespace Utils", () =>
            {
                w.WithCurlyBraces($"public static class StringUtils", () =>
                {
                    w.WithCurlyBraces($"public static string Capitalize(this string s)", () =>
                    {
                        w.WriteLine("return string.Concat(s[0].ToString().ToUpper(), s.ToLower().AsSpan(1));");
                    });

                    w.WithCurlyBraces($"public static string Format(this string s)", () =>
                    {
                        w.WriteLine("var split = s.Split(\".\");");
                        w.WriteLine(" List<string> levels = new();");

                        w.WithCurlyBraces($"foreach (var sp in split)", () =>
                        {
                            w.WriteLine("levels.Add(sp.Capitalize());");
                        });

                        w.WriteLine("return string.Join(\".\", levels);");

                    });

                });

            });

            return (w, $"{tempFilePath}/Domain/Utils/");

        }
    }
}
