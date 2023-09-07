using CodegenCS;
using Serilog;

namespace Domain.Generators
{
    public static class ApigenSelectGenerator
    {
        public static (ICodegenOutputFile, string?) Generator(string tempFilePath)
        {
            string ns = "Utils";
            string cl = "ApigenSelect";
            Log.Debug($"Generate {cl}");

            var ctx = new CodegenContext();
            var w = ctx[$"{cl}.cs"];
            w.WriteLine("using System.Collections;");
            w.WriteLine("using System.Reflection;");
            w.WithCurlyBraces($"namespace {ns}", () =>
            {
                w.WithCurlyBraces($"public static class ApigenSelect", () =>
                {
                    Apply(w);
                    ResolveTypeAndValue(w);
                    ContainInLevels(w);
                });
            });

            return (w, $"{tempFilePath}/Domain/{ns}/");
        }

        public static void Apply(ICodegenOutputFile w)
        {
            w.WithCurlyBraces($"public static void Apply<T>(IQueryable<T> result, List<string> select) where T : class", () =>
            {
                w.WithCurlyBraces($"foreach (var item in result)", () =>
                {
                    w.WriteLine("ResolveTypeAndValue<T>(item, select, typeof(T));");
                });
            });
        }

        public static void ResolveTypeAndValue(ICodegenOutputFile w)
        {
            w.WithCurlyBraces($"public static void ResolveTypeAndValue<T>(object? obj, List<string> select, Type realType, int iteration = 0)  where T : class", () =>
            {
                w.WriteLine("if (obj == null) return;")
                .WriteLine("var type = obj.GetType();")
                .WriteLine("if (type.Equals(realType)) iteration++;")
                .WriteLine("var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);")
                .WriteLine("if (iteration > properties.Length) return;");


                w.WithCurlyBraces($"foreach (var p in properties)", () =>
                {
                    w.WithCurlyBraces($"if (p.PropertyType.Name.Contains(\"List\"))", () =>
                    {
                        w.WriteLine("var currentObj = p.GetValue(obj) as IEnumerable;")
                        .WithCurlyBraces($"if (currentObj != null)", () =>
                        {
                            w.WithCurlyBraces($"foreach (var l in currentObj)", () =>
                            {
                                w.WriteLine("ResolveTypeAndValue<T>(l, select, l.GetType(), iteration++);");
                            });
                        });
                    });
                    w.WithCurlyBraces($"if (p.PropertyType.IsClass && p.PropertyType != typeof(string))", () =>
                    {
                        w.WriteLine("var currentObj = p.GetValue(obj);")
                        .WithCurlyBraces($"if (currentObj != null)", () =>
                        {
                            w.WriteLine("ResolveTypeAndValue<T>(currentObj, select, currentObj.GetType(), iteration++);");
                        });
                    });
                    w.WithCurlyBraces($"else if (p.Name.Contains(\"Capacity\") || p.Name.Contains(\"Count\"))", () =>
                    {
                        w.WriteLine("return;");
                    });
                    w.WithCurlyBraces($"else if (!ContainInLevels(select, p, typeof(T)))", () =>
                    {
                        w.WriteLine("p.SetValue(obj, null);")
                        .WriteLine("GC.Collect();");
                    });
                });
            });
        }

        public static void ContainInLevels(ICodegenOutputFile w)
        {
            w.WithCurlyBraces($"private static bool ContainInLevels(List<string> select, PropertyInfo info, Type T)", () =>
            {
                w.WithCurlyBraces($"foreach (var s in select)", () =>
                {
                    w.WriteLine("var split = s.Split('.');")
                    .WithCurlyBraces($"if (split.Length > 1)", () =>
                    {
                        w.WithCurlyBraces($"if (info.ReflectedType != null && split[^2].Contains(info.ReflectedType.Name, StringComparison.InvariantCultureIgnoreCase) && split[^1].Contains(info.Name, StringComparison.InvariantCultureIgnoreCase))", () =>
                         {
                             w.WriteLine("return true;");
                         });
                    })
                    .WithCurlyBraces($"else if (info.ReflectedType != null && split[0].Contains(info.Name, StringComparison.InvariantCultureIgnoreCase) && T.Name.Contains(info.ReflectedType.Name))", () =>
                    {
                        w.WriteLine("return true;");
                    });
                });

                w.WriteLine(" return false;");
            });
        }


    }
}
