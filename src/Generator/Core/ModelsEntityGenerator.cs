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
            string projectName = OpenApiUtils.GetProjectName(doc);
            string ns = $"{projectName}.Infrastructure.Entities";

            var apigenModels = OpenApiUtils.GetApiGenModelsOrDefault(doc);

            if (apigenModels != null)
            {
                foreach (var entity in apigenModels)
                {
                    GenerateEntity(entity, ns, tempFilePath, apigenModels).SaveToFile(save);
                }
            }

        }

        public static (ICodegenOutputFile, string?) GenerateEntity(KeyValuePair<string, IOpenApiAny> entity, string ns, string tempFilePath, OpenApiObject? allEntities = null)
        {
            string cl = $"{entity.Key.Pascalize()}";
            var entityObject = (OpenApiObject)entity.Value;

            var ctx = new CodegenContext();
            var w = ctx[$"{cl}.cs"];
            w.WriteLine("using System.ComponentModel.DataAnnotations;");
            w.WriteLine("using System.ComponentModel.DataAnnotations.Schema;\n");

            var entityRelationalPersistence = (OpenApiObject)entityObject.FirstOrDefault(x => x.Key.Equals("relational-persistence")).Value;
            string? tableName = (entityRelationalPersistence?.FirstOrDefault(x => x.Key.Equals("table", StringComparison.Ordinal)).Value as OpenApiString)?.Value;

            w.WithCurlyBraces($"namespace {ns}", () =>
            {
                if (tableName != null)
                    w.WriteLine($"[Table(\"{tableName}\")]");
                w.WithCurlyBraces($"public class {cl}", () =>
                {
                    var attribute = entityObject.FirstOrDefault(x => x.Key.Equals("attributes"));
                    WriteProperty(w, attribute.Value, allEntities);
                });
            });

            return (w, $"{tempFilePath}/src/Infrastructure/Entities/");
        }

        private static void WriteProperty(ICodegenOutputFile w, IOpenApiAny propertyValue, OpenApiObject? allEntities = null)
        {
            if (propertyValue is OpenApiArray propertyArray)
            {
                foreach (IOpenApiAny propertieAny in propertyArray)
                {
                    if (propertieAny is OpenApiObject propertie)
                    {
                        WritePropertyDetails(w, propertie, allEntities: allEntities);
                    }
                }
            }
            else if (propertyValue is OpenApiObject propertyObject)
            {
                foreach (var propertie in propertyObject)
                {
                    if (propertie.Value is OpenApiObject propertyDetails)
                    {
                        WritePropertyDetails(w, propertyDetails, propertie.Key, allEntities);
                    }
                }
            }
        }

        private static void WritePropertyDetails(ICodegenOutputFile w, OpenApiObject propertie, string? name = null, OpenApiObject? allEntities = null)
        {
            name ??= ((OpenApiString)propertie.FirstOrDefault(x => x.Key.Equals("name")).Value).Value;
            var type = (OpenApiString)propertie.FirstOrDefault(x => x.Key.Equals("type")).Value;
            var itemsType = (OpenApiString)propertie.FirstOrDefault(x => x.Key.Equals("items-type")).Value;
            var relationalPersistence = (OpenApiObject)propertie.FirstOrDefault(x => x.Key.Equals("relational-persistence")).Value;

            bool isArray = type.Value.Equals("Array", StringComparison.OrdinalIgnoreCase);
            OpenApiString? column = null;
            OpenApiString? foreignColumn = null;
            bool isFkRelation = false;

            if (relationalPersistence != null)
            {
                var key = (OpenApiBoolean)relationalPersistence.FirstOrDefault(x => x.Key.Equals("primary-key")).Value;
                column = (OpenApiString)relationalPersistence.FirstOrDefault(x => x.Key.Equals("column")).Value;
                foreignColumn = (OpenApiString)relationalPersistence.FirstOrDefault(x => x.Key.Equals("foreign-column")).Value;

                if (key != null && key.Value)
                {
                    w.WriteLine("[Key]");
                    w.WriteLine("[DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
                }

                // Many-to-one FK column: write a typed FK property so EF Core maps it correctly
                if (column != null && !column.Value.Pascalize().Equals(name.Pascalize(), StringComparison.OrdinalIgnoreCase))
                {
                    isFkRelation = true;
                    string fkType = GetEntityPrimaryKeyType(allEntities, type.Value) ?? "long?";
                    w.WriteLine($"[Column(\"{column.Value}\")]");
                    w.WriteLine($"public {fkType} {column.Value.Pascalize()} {{get; set;}}");
                }
            }

            // Add [Column] for scalar/non-collection properties (no separate FK property was written above)
            if (!isArray && foreignColumn == null && !isFkRelation)
            {
                string columnName = column?.Value ?? name.Underscore();
                w.WriteLine($"[Column(\"{columnName}\")]");
            }

            // Link the navigation property to the FK property so EF Core doesn't create a shadow property
            if (isFkRelation && column != null)
                w.WriteLine($"[ForeignKey(nameof({column.Value.Pascalize()}))]");

            w.WriteLine($"public {FormatTypeEntity(type.Value, itemsType)} {name.Pascalize()} {{get; set;}}");
        }

        private static string? GetEntityPrimaryKeyType(OpenApiObject? allEntities, string entityTypeName)
        {
            if (allEntities == null || !allEntities.TryGetValue(entityTypeName, out var entityValue))
                return null;

            if (entityValue is not OpenApiObject entityObj)
                return null;

            if (entityObj.FirstOrDefault(x => x.Key.Equals("attributes")).Value is not OpenApiArray attributes)
                return null;

            foreach (var attr in attributes)
            {
                if (attr is not OpenApiObject attrObj) continue;

                var relPersistence = attrObj.FirstOrDefault(x => x.Key.Equals("relational-persistence")).Value as OpenApiObject;
                if (relPersistence == null) continue;

                var isPk = relPersistence.FirstOrDefault(x => x.Key.Equals("primary-key")).Value as OpenApiBoolean;
                if (isPk?.Value != true) continue;

                var pkType = (attrObj.FirstOrDefault(x => x.Key.Equals("type")).Value as OpenApiString)?.Value;
                return FormatType(pkType);
            }

            return null;
        }


    }
}
