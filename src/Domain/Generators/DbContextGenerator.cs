﻿using CodegenCS;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;
using static Domain.Utils.StringUtils;

namespace Domain.Generators
{
    public static class DbContextGenerator
    {
        private static readonly string cl = "ApiDbContext";

        public static (ICodegenOutputFile, string?) Generator(OpenApiDocument doc, string tempFilePath)
        {
            Log.Debug($"Generate {cl}");
            var apigenModels = (OpenApiObject)doc.Components.Extensions.FirstOrDefault(x => x.Key.Equals("x-apigen-models")).Value;
            var ctx = new CodegenContext();
            var w = ctx[$"{cl}.cs"];

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
                            string cl = $"{FormatName(entity.Key)}";
                            w.WriteLine($"public DbSet<{cl}Entity> {cl} {{get; set;}}");
                        }
                        w.WithCurlyBraces($"\nprotected override void OnModelCreating(ModelBuilder modelBuilder)", () =>
                        {
                            foreach (var entity in apigenModels)
                            {
                                string cl = $"{FormatName(entity.Key)}";
                                w.WithCurlyBraces($"modelBuilder.Entity<{cl}Entity>(entity =>", () =>
                                {
                                    var attribute = ((OpenApiObject)entity.Value).FirstOrDefault(x => x.Key.Equals("attributes"));
                                    var relationalPersistenceTable = ((OpenApiObject)entity.Value).FirstOrDefault(x => x.Key.Equals("relational-persistence"));

                                    var t = (OpenApiObject)relationalPersistenceTable.Value;
                                    if (t != null)
                                    {
                                        var table = (OpenApiString)t.FirstOrDefault(x => x.Key.Equals("table")).Value;
                                        if (table != null)
                                            w.WriteLine($"entity.ToTable(\"{table.Value}\");");

                                        foreach (OpenApiObject propertie in (OpenApiArray)attribute.Value)
                                        {

                                            var name = (OpenApiString)propertie.FirstOrDefault(x => x.Key.Equals("name")).Value;
                                            var type = (OpenApiString)propertie.FirstOrDefault(x => x.Key.Equals("type")).Value;
                                            //var validations = (OpenApiArray)propertie.FirstOrDefault(x => x.Key.Equals("validations")).Value;
                                            var relationalPersistence = (OpenApiObject)propertie.FirstOrDefault(x => x.Key.Equals("relational-persistence")).Value;

                                            if (relationalPersistence != null)
                                            {
                                                //var key = (OpenApiBoolean)relationalPersistence.FirstOrDefault(x => x.Key.Equals("primary-key")).Value;
                                                //var autogenerated = (OpenApiBoolean)relationalPersistence.FirstOrDefault(x => x.Key.Equals("autogenerated")).Value;
                                                var column = (OpenApiString)relationalPersistence.FirstOrDefault(x => x.Key.Equals("column")).Value;
                                                //var foreignColumn = (OpenApiString)relationalPersistence.FirstOrDefault(x => x.Key.Equals("foreign-column")).Value;
                                                //var intermediateTable = (OpenApiString)relationalPersistence.FirstOrDefault(x => x.Key.Equals("intermediateTable")).Value;


                                                if (!type.Value.Contains("Array"))
                                                {
                                                    if (column == null)
                                                    {
                                                        w.WriteLine($"entity.Property(e => e.{FormatName(name.Value)})");
                                                        w.WriteLine($".HasColumnName(\"{name.Value.ToSnakeCase()}\");");
                                                    }
                                                    else
                                                    {
                                                        w.WriteLine($"entity.Property(e => e.{FormatName(column.Value)})");
                                                        w.WriteLine($".HasColumnName(\"{column.Value}\");");

                                                        if (table != null)
                                                        {
                                                            w.WriteLine($"entity.HasOne(e => e.{FormatName(name.Value)})");
                                                            w.WriteLine($".WithMany(p => p.{FormatName(table.Value)})");
                                                            w.WriteLine($".HasForeignKey(d => d.{FormatName(column.Value)});");
                                                        }

                                                    }

                                                }
                                            }
                                            else
                                            {
                                                w.WriteLine($"entity.Property(e => e.{FormatName(name.Value)})");
                                                w.WriteLine($".HasColumnName(\"{name.Value.ToSnakeCase()}\");");
                                            }
                                        }
                                    }

                                });
                                w.Write(");\nOnModelCreatingPartial(modelBuilder);");

                            }
                        });
                        w.WriteLine("partial void OnModelCreatingPartial(ModelBuilder modelBuilder);");
                    }

                });
            });

            return (w, $"{tempFilePath}/Infrastructure/");

        }

    }

}

