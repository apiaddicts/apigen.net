using CodegenCS;
using Generator.Utils;
using Humanizer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;
using static Generator.Utils.FileUtils;
using static Generator.Utils.StringUtils;

namespace Generator.Core
{
    public static class ModelsEntityGenerator
    {
        /// <summary>
        /// One entity is generated for each `x-apigen-models` tag defined.
        /// If they do not exist, this process is ignored.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="tempFilePath"></param>
        /// <param name="save"></param>
        public static void Generator(OpenApiDocument doc, string tempFilePath, bool save = true)
        {

            Log.Debug($"Generating ~ Entity Models");
            string ns = "Entities";

            var apigenModels = OpenApiUtils.GetApiGenModelsOrDefault(doc);

            if (apigenModels != null)
            {
                foreach (var entity in apigenModels)
                {
                    GenerateEntity(entity, ns, tempFilePath).SaveToFile(save);
                }
            }

        }

        public static (ICodegenOutputFile, string?) GenerateEntity(KeyValuePair<string, IOpenApiAny> entity, string ns, string tempFilePath)
        {
            string cl = $"{entity.Key.Pascalize()}";

            var ctx = new CodegenContext();
            var w = ctx[$"{cl}.cs"];
            w.WriteLine("using System.ComponentModel.DataAnnotations;");
            w.WriteLine("using System.ComponentModel.DataAnnotations.Schema;\n");

            w.WithCurlyBraces($"namespace {ns}", () =>
            {
                w.WithCurlyBraces($"public class {cl}", () =>
                {
                    var attribute = ((OpenApiObject)entity.Value).FirstOrDefault(x => x.Key.Equals("attributes"));
                    WriteProperty(w, attribute.Value);
                });
            });

            return (w, $"{tempFilePath}/src/Infrastructure/{ns}/");
        }

        private static void WriteProperty(ICodegenOutputFile w, IOpenApiAny propertyValue)
        {
            if (propertyValue is OpenApiArray propertyArray)
            {
                foreach (IOpenApiAny propertieAny in propertyArray)
                {
                    if (propertieAny is OpenApiObject propertie)
                    {
                        WritePropertyDetails(w, propertie);
                    }
                }
            }
            else if (propertyValue is OpenApiObject propertyObject)
            {
                foreach (var propertie in propertyObject)
                {
                    if (propertie.Value is OpenApiObject propertyDetails)
                    {
                        WritePropertyDetails(w, propertyDetails, propertie.Key);
                    }
                }
            }
        }

        private static void WritePropertyDetails(ICodegenOutputFile w, OpenApiObject propertie, string? name = null)
        {
            name ??= ((OpenApiString)propertie.FirstOrDefault(x => x.Key.Equals("name")).Value).Value;
            var type = (OpenApiString)propertie.FirstOrDefault(x => x.Key.Equals("type")).Value;
            var itemsType = (OpenApiString)propertie.FirstOrDefault(x => x.Key.Equals("items-type")).Value;
            OpenApiString? column = null;
            var relationalPersistence = (OpenApiObject)propertie.FirstOrDefault(x => x.Key.Equals("relational-persistence")).Value;

            if (relationalPersistence != null)
            {
                var key = (OpenApiBoolean)relationalPersistence.FirstOrDefault(x => x.Key.Equals("primary-key")).Value;
                column = (OpenApiString)relationalPersistence.FirstOrDefault(x => x.Key.Equals("column")).Value;

                if (key != null && key.Value)
                {
                    w.WriteLine("[Key]");
                    w.WriteLine("[DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
                }

                if (column != null)
                    w.WriteLine($"public {FormatType(column.PrimitiveType.ToString())} {column.Value.Pascalize()} {{get; set;}}");
            }

            w.WriteLine($"public {FormatTypeEntity(type.Value, itemsType)} {name.Pascalize()} {{get; set;}}");
        }


    }
}
