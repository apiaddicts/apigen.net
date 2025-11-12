using CodegenCS;
using Generator.Utils;
using Humanizer;
using Microsoft.OpenApi.Models;
using Serilog;

namespace Generator.Core
{
    public static class DbContextGenerator
    {
        private static readonly string cl = "ApiDbContext";

        /// <summary>
        /// The generation of the context requires the `x-apigen-models` extension tag in the OpenApi for each entity. 
        /// This process is not intended to be definitive, an ORM would be needed after generation.
        /// If they do not exist, this process is ignored.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="tempFilePath"></param>
        public static (ICodegenOutputFile, string?) Generator(OpenApiDocument doc, string tempFilePath)
        {
            Log.Debug("Adding ~ {Class}", cl);
            var ctx = new CodegenContext();
            var w = ctx[$"{cl}.cs"];

            var apigenModels = OpenApiUtils.GetApiGenModelsOrDefault(doc);
            string ns = "Context";
            if (apigenModels != null)
                w.WriteLine("using Entities;");
            w.WriteLine("using Microsoft.EntityFrameworkCore;\n");
            w.WithCurlyBraces($"namespace {ns}", () =>
            {
                w.WithCurlyBraces($"public partial class {cl} : DbContext", () =>
                {
                    w.WriteLine($"public {cl}(DbContextOptions<{cl}> options) : base(options) {{ }}\n");

                    w.WriteLine($"public {cl}() {{ }}");

                    if (apigenModels != null)
                    {
                        foreach (var entity in apigenModels)
                        {
                            string e = $"{entity.Key.Pascalize()}";
                            w.WriteLine($"public DbSet<{e}> {e} {{get; set;}}");
                        }
                        w.WithCurlyBraces($"\nprotected override void OnModelCreating(ModelBuilder modelBuilder)", () =>
                        {
                            foreach (var entity in apigenModels)
                            {
                                string e = $"{entity.Key.Pascalize()}";
                                w.WithCurlyBraces($"modelBuilder.Entity<{e}>(entity =>", () =>
                                {
                                });
                                w.Write(");\nOnModelCreatingPartial(modelBuilder);");

                            }
                        });
                        w.WriteLine("partial void OnModelCreatingPartial(ModelBuilder modelBuilder);");
                    }

                });
            });

            return (w, $"{tempFilePath}/src/Infrastructure/");

        }

    }

}

