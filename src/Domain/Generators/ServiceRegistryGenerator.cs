using CodegenCS;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;
using static Domain.Utils.StringUtils;

namespace Domain.Generators
{
    public static class ServiceRegistryGenerator
    {
        private static string cl = "ServiceRegistry";

        public static (ICodegenOutputFile, string?) Generator(OpenApiDocument doc, string tempFilePath)
        {
            Log.Debug($"Generate {cl}");
            var apigenModels = (OpenApiObject)doc.Components.Extensions.FirstOrDefault(x => x.Key.Equals("x-apigen-models")).Value;

            var ctx = new CodegenContext();
            var w = ctx[$"{cl}.cs"];
            w.WriteLine("using Repositories;");
            w.WriteLine("using Services;\n");
            w.WithCurlyBraces($"namespace Microsoft.Extensions.DependencyInjection", () =>
            {
                w.WithCurlyBraces($"public static class {cl}", () =>
                {
                    w.WithCurlyBraces($"public static IServiceCollection AddRepositories(this IServiceCollection repositories)", () =>
                    {

                        if (apigenModels != null)
                        {
                            foreach (var entity in apigenModels)
                            {
                                string cl = $"{FormatName(entity.Key)}";
                                w.WriteLine($"repositories.AddTransient<{cl}Repository>();");

                            }
                        }
                        w.WriteLine($"return repositories;");
                    });

                    w.WithCurlyBraces($"public static IServiceCollection AddServices(this IServiceCollection services)", () =>
                    {
                        if (apigenModels != null)
                        {
                            foreach (var entity in apigenModels)
                            {
                                string cl = $"{FormatName(entity.Key)}";
                                w.WriteLine($"services.AddTransient<{cl}Service>();");
                            }
                        }
                        w.WriteLine($"return services;");
                    });

                });
            });

            return (w, $"{tempFilePath}/Api/Helpers/");

        }

    }
}
