using CodegenCS;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;
using static Domain.Utils.StringUtils;

namespace Domain.Generators
{
    public static class MappingProfileGenerator
    {
        private static string cl = "MappingProfile";

        public static (ICodegenOutputFile, string?) Generator(OpenApiDocument doc, string tempFilePath)
        {
            Log.Debug($"Generate Mappings");
            var ctx = new CodegenContext();
            var w = ctx[$"{cl}.cs"];
            w.WriteLine("using AutoMapper;");

            var apigenModels = (OpenApiObject)doc.Components.Extensions.FirstOrDefault(x => x.Key.Equals("x-apigen-models")).Value;

            if (apigenModels != null)
                w.WriteLine("using Entities;");

            w.WriteLine("using Models;");
            w.WithCurlyBraces($"namespace Helpers", () =>
            {
                w.WithCurlyBraces($"public class {cl} : Profile", () =>
                {
                    w.WithCurlyBraces($"public {cl}()", () =>
                {

                    foreach (var schema in doc.Components.Schemas)
                    {
                        string dto = $"{FormatName(schema.Key)}";
                        var entity = ReadModelInExtensions(schema);
                        if (entity != null)
                        {
                            w.WriteLine($"CreateMap<Models.{dto}Model, Entities.{entity.Value}Entity>();");
                        }

                    }

                });
                });

            });
            return (w, $"{tempFilePath}/Api/Helpers/");
        }

        private static OpenApiString? ReadModelInExtensions(KeyValuePair<string, OpenApiSchema> schema)
        {
            var extensions = (OpenApiObject)schema.Value.Extensions.FirstOrDefault(x => x.Key.Equals("x-apigen-mapping")).Value;
            if (extensions != null)
            {
                return (OpenApiString)extensions.FirstOrDefault(x => x.Key.Equals("model")).Value;
            }

            return null;
        }
    }
}
