using CodegenCS;
using Domain.Utils;
using Microsoft.OpenApi.Models;
using Serilog;
using static Domain.Utils.StringUtils;

namespace Domain.Generators
{
    public static class ModelsDtoGenerator
    {
        public static void Generator(OpenApiDocument doc, string tempFilePath, bool save = true)
        {
            Log.Debug($"Generate Dto Models");
            string ns = "Models";
            foreach (var schema in doc.Components.Schemas)
            {
                GenerateController(schema, ns, tempFilePath).SaveToFile(save);
            }
        }

        public static (ICodegenOutputFile, string?) GenerateController(KeyValuePair<string, OpenApiSchema> schema,
            string ns, string tempFilePath)
        {
            string cl = $"{FormatName(schema.Key)}Model";
            var ctx = new CodegenContext();
            var w = ctx[$"{cl}.cs"];

            w.WithCurlyBraces($"namespace {ns}", () =>
            {
                var extend = "";
                if (schema.Value.AllOf.Any())
                {
                    foreach (var reference in schema.Value.AllOf)
                    {
                        if (reference.Reference != null)
                        {
                            extend = $" : {FormatName(reference.Reference.Id)}Model";
                        }
                    }
                }
                w.WithCurlyBraces($"public class {cl}{extend}", () =>
                {


                    foreach (var reference in schema.Value.AllOf)
                    {
                        if (reference.Properties != null)
                        {
                            WriteProperties(w, reference.Properties, extend);
                        }
                    }
                    WriteProperties(w, schema.Value.Properties, extend);
                });
            });

            return (w, $"{tempFilePath}/Domain/Models/");
        }

        public static void WriteProperties(ICodegenOutputFile w, IDictionary<string,
            OpenApiSchema> properties, string extend)
        {
            foreach (var propertie in properties)
            {
                string field = $"{FormatName(propertie.Key)}";
                var type = FormatType(propertie.Value.Type, propertie.Value.Format);

                if (propertie.Value.Type!=null && !propertie.Value.Type.Equals("object"))
                {
                    w.WriteLine($"public {type} {field} {{get; set;}}");
                }
                else if (propertie.Value.Reference != null)
                {
                    w.WriteLine($"public List<{FormatName(propertie.Value.Reference.Id)}Model> {field} {{get; set;}}");
                }
                else if (propertie.Value.Properties != null && extend.Equals(""))
                {
                    w.WriteLine($"public {field}Dto {field} {{get; set;}}");
                    w.WithCurlyBraces($"public class {field}Dto", () =>
                    {
                        foreach (var p in propertie.Value.Properties)
                        {
                            field = $"{FormatName(p.Key)}";
                            type = FormatType(p.Value.Type);
                            w.WriteLine($"public {type} {field} {{get; set;}}");
                        }
                    });
                }

            }

        }

    }


}
