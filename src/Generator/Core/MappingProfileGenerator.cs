using CodegenCS;
using Humanizer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;

namespace Generator.Core
{
    public static class MappingProfileGenerator
    {
        private static readonly string cl = "MappingProfile";

        /// <summary>
        /// Mapping profile generation requires the `x-apigen-models` extension tag for each 
        /// entity and `x-apigen-mapping` on each defined input model.
        /// If they do not exist, this process is ignored.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="tempFilePath"></param>
        public static (ICodegenOutputFile, string?) Generator(OpenApiDocument doc, string tempFilePath)
        {

            Log.Debug($"Generating ~ Mappings");
            var ctx = new CodegenContext();
            var w = ctx[$"{cl}.cs"];
            w.WriteLine("using AutoMapper;");

            w.WithCurlyBraces($"namespace Helpers", () =>
            {
                w.WithCurlyBraces($"public class {cl} : Profile", () =>
                {
                    w.WithCurlyBraces($"public {cl}()", () =>
                {
                    if (doc.Components != null)
                    {
                        foreach (var schema in doc.Components.Schemas)
                        {
                            string dto = $"{schema.Key.Pascalize()}";
                            var entity = ReadModelInExtensions(schema);
                            if (entity != null)
                            {
                                w.WriteLine($"CreateMap<Models.{dto}, Entities.{entity.Value}>();");
                            }

                        }
                    }

                });
                });

            });
            return (w, $"{tempFilePath}/src/Api/Helpers/");
        }

        private static OpenApiString? ReadModelInExtensions(KeyValuePair<string, OpenApiSchema> schema)
        {
            var extensions = (OpenApiObject)schema.Value.Extensions
                .FirstOrDefault(x => x.Key.Equals("x-apigen-mapping")).Value;
            if (extensions != null)
            {
                return (OpenApiString)extensions.FirstOrDefault(x => x.Key.Equals("model")).Value;
            }

            return null;
        }
    }
}
