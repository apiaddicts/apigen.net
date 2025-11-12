using CodegenCS;
using Generator.Utils;
using Humanizer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;

namespace Generator.Core
{
    public static class ServiceRegistryGenerator
    {
        private static readonly string className = "ServiceRegistry";

        public static (ICodegenOutputFile, string?) Generator(OpenApiDocument doc, string tempFilePath)
        {
            Log.Debug("Generating Service Registry");


            var context = new CodegenContext();
            var writer = context[$"{className}.cs"];
            writer.WriteLine("using Repositories;");
            writer.WriteLine("using Services;\n");
            var apiGenModels = OpenApiUtils.GetApiGenModelsOrDefault(doc);
            writer.WithCurlyBraces($"namespace Microsoft.Extensions.DependencyInjection", () =>
            {
                writer.WithCurlyBraces($"public static class {className}", () =>
                { 
                    AddEntities(writer, apiGenModels!, "Repository");
                    AddEntities(writer, apiGenModels!, "Service");
                });
            });

            return (writer, $"{tempFilePath}/src/Api/Helpers/");
        }

        private static void AddEntities(ICodegenTextWriter writer, OpenApiObject apiGenModels, string type)
        {
            writer.WithCurlyBraces($"public static IServiceCollection Add{type.Pluralize()}(this IServiceCollection services)", () =>
            {
                if (apiGenModels != null)
                {
                    foreach (var entity in apiGenModels)
                    {
                        string entityName = $"{entity.Key.Pascalize()}{type}";
                        writer.WriteLine($"services.AddTransient<{entityName}>();");
                    }
                }
                writer.WriteLine("return services;");
            });
        }
    }
}
