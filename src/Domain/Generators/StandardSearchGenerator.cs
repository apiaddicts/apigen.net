using CodegenCS;
using Serilog;

namespace Domain.Generators
{
    public static class StandardSearchGenerator
    {
        public static (ICodegenOutputFile, string?) Generator(string tempFilePath)
        {
            string ns = "Models";
            string cl = "StandardSearchModel";
            Log.Debug($"Generate {cl}");

            var ctx = new CodegenContext();
            var w = ctx[$"{cl}.cs"];
            w.WriteLine("using System.Text.Json;");
            w.WriteLine("using System.Text.Json.Serialization;");
            w.WithCurlyBraces($"namespace {ns}", () =>
            {
                Root(w);
                FilterValueClass(w);
                FilterClass(w);
                OperationEnum(w);
            });

            return (w, $"{tempFilePath}/Domain/{ns}/");
        }

        public static void Root(ICodegenOutputFile w)
        {

            w.WithCurlyBraces($"public class StandardSearchModel", () =>
            {
                w.WriteLine("public Filter? Filter { get; set; }");

                w.WithCurlyBraces($"public static List<(string Operation, bool SingleValue, string Symbol)> FilterOperation = new()", () =>
                {
                    w.WriteLine("(OperationEnum.AND.ToString(), false, \"&&\"),");
                    w.WriteLine("(OperationEnum.OR.ToString(), false, \"or\"),");
                    w.WriteLine("(OperationEnum.GT.ToString(), true, \">\"),");
                    w.WriteLine("(OperationEnum.LT.ToString(), true, \"<\"),");
                    w.WriteLine("(OperationEnum.GTEQ.ToString(), true, \">=\"),");
                    w.WriteLine("(OperationEnum.LTEQ.ToString(), true, \"<=\"),");
                    w.WriteLine("(OperationEnum.EQ.ToString(), true, \"==\"),");
                    w.WriteLine("(OperationEnum.NEQ.ToString(), true, \"!=\"),");
                    w.WriteLine("(OperationEnum.IN.ToString(), false, \"\"),");
                    w.WriteLine("(OperationEnum.BETWEEN.ToString(), false, \"\"),");
                    w.WriteLine("(OperationEnum.SUBSTRING.ToString(), true, \"\"),");
                    w.WriteLine("(OperationEnum.LIKE.ToString(), true, \"\"),");
                    w.WriteLine("(OperationEnum.ILIKE.ToString(), true, \"\"),");
                    w.WriteLine("(OperationEnum.NLIKE.ToString(), true, \"\"),");
                    w.WriteLine("(OperationEnum.REGEXP.ToString(), true, \"\"),");

                }).Write(";");

                w.WithCurlyBraces("public static string SymbolQuery(OperationEnum o)", () =>
                {
                    w.WriteLine("return FilterOperation.Find(x => x.Operation.Equals(o.ToString())).Symbol;");
                });

                w.WithCurlyBraces("public static string Format(string property, OperationEnum o, JsonElement? value)", () =>
                {
                    w.WithCurlyBraces("switch (o)", () =>
                    {
                        w.WriteLine("case OperationEnum.AND:")
                        .WriteLine("case OperationEnum.OR:")
                        .WriteLine("case OperationEnum.GT:")
                        .WriteLine("case OperationEnum.LT:")
                        .WriteLine("case OperationEnum.GTEQ:")
                        .WriteLine("case OperationEnum.LTEQ:")
                        .WriteLine("case OperationEnum.EQ:")
                        .WithCurlyBraces("case OperationEnum.NEQ:", () =>
                        {
                            w.WriteLine("return $\"{property} {SymbolQuery(o)} {isString(value)}\";");
                        })
                        .WithCurlyBraces("case OperationEnum.SUBSTRING:", () =>
                        {
                            w.WriteLine("return $\"{property}.Contains({isString(value)})\";");
                        })
                        .WithCurlyBraces("case OperationEnum.LIKE:", () =>
                        {
                            w.WriteLine("return $\"{property}.{TranslateLogicLikeSqlToDotnet(value)})\";");
                        })
                        .WithCurlyBraces("case OperationEnum.ILIKE:", () =>
                        {
                            w.WriteLine("return $\"{property}.ToLower().{TranslateLogicLikeSqlToDotnet(value)}.ToLower())\";");
                        })
                        .WithCurlyBraces("case OperationEnum.NLIKE:", () =>
                        {
                            w.WriteLine("return $\"!{property}.{TranslateLogicLikeSqlToDotnet(value)})\";");
                        })
                        .WithCurlyBraces("case OperationEnum.IN:", () =>
                        {
                            w.WithCurlyBraces("if (value != null && value.Value.ValueKind == JsonValueKind.Array)", () =>
                            {
                                w.WriteLine("var array = value.Value.EnumerateArray().ToArray();");
                                w.WriteLine("var query = \"\";");

                                w.WithCurlyBraces("foreach (var a in array)", () =>
                                 {
                                     w.WriteLine("query += $\"{property} == {isString(a)}\";");
                                     w.WriteLine("if (!a.Equals(array.Last())) query += \" or \";");
                                 });
                                w.WriteLine("return query;");

                            })
                            .WriteLine("break;");

                        })
                        .WithCurlyBraces("case OperationEnum.BETWEEN:", () =>
                        {
                            w.WithCurlyBraces("if (value != null && value.Value.ValueKind == JsonValueKind.Array)", () =>
                            {
                                w.WriteLine("var array = value.Value.EnumerateArray().ToArray();");
                                w.WriteLine("return $\"{property} >= {isString(array[0])} && {property} <= {isString(array[1])}\";");
                            }).
                            WriteLine("break;");

                        })
                        .WriteLine("case OperationEnum.REGEXP:")
                        .WriteLine("\tbreak;");
                    })
                    .WriteLine("return \"\";");
                });

                w.WithCurlyBraces("public static string QuerySearchBuilder(OperationEnum operation, List<FilterValue> values, List<string>? query = null)", () =>
                {
                    w.WriteLine("if (query == null) query = new();");

                    w.WithCurlyBraces("foreach (var v in values)", () =>
                    {
                        w.WithCurlyBraces("if (v.Filter != null && v.Filter.Values != null)", () =>
                        {
                            w.WriteLine("query.Add(QuerySearchBuilder(v.Filter.Operation, v.Filter.Values, query));");
                        });
                        w.WithCurlyBraces("if (v.Value != null && v.Property != null)", () =>
                        {
                            w.WriteLine("return $\"{Format(v.Property, operation, v.Value)}\";");
                        });
                    });
                    w.WriteLine("return string.Join($\" {SymbolQuery(operation)} \", query);");
                });


                w.WithCurlyBraces("private static string TranslateLogicLikeSqlToDotnet(JsonElement? value)", () =>
                {

                    w.WriteLine("if (value == null) return \"\";");

                    w.WithCurlyBraces("if (value.Value.ValueKind == JsonValueKind.String)", () =>
                    {
                        w.WriteLine("string valueFormat = isString(value).Replace(\"\\\"\", \"\");");
                        w.WriteLine("var checkFirst = valueFormat[0].Equals('%');");
                        w.WriteLine("var checkEnd = valueFormat[^1].Equals('%');");
                        w.WriteLine("var result = isString(value).Replace(\"%\", \"\");");

                        w.WithCurlyBraces("if (checkFirst && !checkEnd)", () =>
                        {
                            w.WriteLine("return $\"EndsWith({result}\";");
                        });
                        w.WithCurlyBraces("else if (!checkFirst && checkEnd)", () =>
                        {
                            w.WriteLine(" return $\"StartsWith({result}\";");
                        });

                    });

                    w.WriteLine("return $\"Contains({isString(value)}\";");

                });

                w.WithCurlyBraces("private static string isString(JsonElement? value)", () =>
                {
                    w.WithCurlyBraces("if (value != null && value.Value.ValueKind == JsonValueKind.String)", () =>
                    {
                        w.WriteLine("return $\"\\\"{value}\\\"\";");
                    });

                    w.WriteLine("return $\"{value}\";");

                });

            });

        }

        public static void FilterValueClass(ICodegenOutputFile w)
        {

            w.WithCurlyBraces($"public class FilterValue", () =>
            {
                w.WriteLine("public Filter? Filter { get; set; }");
                w.WriteLine("public string? Property { get; set; }");
                w.WriteLine("public JsonElement? Value { get; set; }");
            });
        }

        public static void FilterClass(ICodegenOutputFile w)
        {

            w.WithCurlyBraces($"public class Filter", () =>
            {
                w.WriteLine("public OperationEnum Operation { get; set; }");
                w.WriteLine("public List<FilterValue>? Values { get; set; }");
            });
        }

        public static void OperationEnum(ICodegenOutputFile w)
        {
            w.WriteLine("[JsonConverter(typeof(JsonStringEnumConverter))]");
            w.WithCurlyBraces($"public enum OperationEnum", () =>
            {
                w.WriteLine("AND, OR, GT, LT, GTEQ, LTEQ, EQ, NEQ, IN, BETWEEN, SUBSTRING, LIKE, ILIKE, NLIKE, REGEXP");
            });

        }
    }
}
