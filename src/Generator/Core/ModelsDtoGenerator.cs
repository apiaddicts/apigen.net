using CodegenCS;
using Generator.Utils;
using Humanizer;
using Microsoft.OpenApi.Models;
using Serilog;
using static Generator.Utils.FileUtils;
using static Generator.Utils.StringUtils;

namespace Generator.Core
{
    public static class ModelsDtoGenerator
    {
        /// <summary>
        /// Generates an input model for each 'AllOf' defined.
        /// Ignores models containing the word "Standard" or those without properties.
        /// </summary>
        public static void Generator(OpenApiDocument doc, string tempFilePath, bool save = true)
        {
            if(doc.Components != null)
            {
                Log.Debug("Generating Dto Models");
                foreach (var schema in doc.Components.Schemas)
                {
                    if ((schema.Value.Properties.Count > 0 || schema.Value.AllOf.Count > 0) &&
                        !schema.Key.Pascalize().Contains("Standard", StringComparison.CurrentCultureIgnoreCase))
                    {
                        GenerateModels(schema, "Models", tempFilePath).SaveToFile(save);
                    }
                }
            }
        }

        private static (ICodegenOutputFile, string?) GenerateModels(KeyValuePair<string, OpenApiSchema> schema,
    string ns, string tempFilePath)
        {
            string className = schema.Key.Pascalize();
            var context = new CodegenContext();
            var writer = context[$"{className}.cs"];

            writer.WriteLine("using System.ComponentModel.DataAnnotations;");
            writer.WriteLine("using System.Text.Json.Serialization;");

            writer.WithCurlyBraces($"namespace {ns}", () =>
            {
                var allOfWithRef = schema.Value.AllOf.Where(x => x.Reference != null).ToList();
                string? baseClass = allOfWithRef.Count != 0 ? allOfWithRef[0].Reference.Id.Pascalize() : null;

                string classDeclaration = $"public class {className}" + (baseClass != null ? $" : {baseClass}" : "");
                writer.WithCurlyBraces(classDeclaration, () =>
                {
                    // Write properties for AllOf references without a specific reference object
                    foreach (var reference in schema.Value.AllOf.Where(x => x.Reference == null))
                    {
                        WriteProperties(writer, reference);
                    }
                    // Write direct properties of the schema
                    WriteProperties(writer, schema.Value);
                });
            });

            return (writer, $"{tempFilePath}/src/Domain/Models/");
        }

        private static void WriteProperties(ICodegenOutputFile writer, OpenApiSchema schema)
        {
            foreach (var property in schema.Properties)
            {
                string propertyName = property.Key.Pascalize();
                string type = GetType(property.Value);

                writer.WriteLine($"[JsonPropertyName(\"{property.Key}\")]");
                if (schema.Required.Contains(property.Key))
                    writer.WriteLine("[Required]");

                WriteValidations(writer, property.Value);

                writer.WriteLine($"public {type} {propertyName} {{ get; set; }}");
            }
        }

        private static string GetType(OpenApiSchema property)
        {
            if (property.Reference != null)
                return $"{property.Reference.Id.Pascalize()}";
            if (property.Items != null)
                return property.Items.Reference != null ? $"List<{property.Items.Reference.Id.Pascalize()}>" : "List";
            return FormatType(property.Type);
        }

        private static void WriteValidations(ICodegenOutputFile writer, OpenApiSchema schema)
        {
            if (schema.MaxLength != null && schema.MinLength != null)
                writer.WriteLine($"[StringLength({schema.MaxLength}, MinimumLength = {schema.MinLength})]");
            if (schema.Maximum != null && schema.Minimum != null)
                writer.WriteLine($"[Range({schema.Minimum}, {schema.Maximum})]");
            if (schema.Format != null && schema.Type == "string")
                writer.WriteLine("[DataType(DataType.Date)]");
        }
    }
}
