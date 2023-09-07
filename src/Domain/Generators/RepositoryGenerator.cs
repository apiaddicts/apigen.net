using CodegenCS;
using Domain.Utils;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;
using static Domain.Utils.StringUtils;

namespace Domain.Generators
{
    public static class RepositoryGenerator
    {
        private static string ns = "Repositories";

        public static void Generator(OpenApiDocument doc, string tempFilePath, bool save = true)
        {
            Log.Debug($"Generate Repositories");
            IBaseRepositoryGenerator(tempFilePath).SaveToFile();
            BaseRepositoryGenerator(tempFilePath).SaveToFile();
            var apigenModels = (OpenApiObject)doc.Components.Extensions.FirstOrDefault(x => x.Key.Equals("x-apigen-models")).Value;

            if (apigenModels != null)
            {
                foreach (var entity in apigenModels)
                {
                    GenerateRepository(entity, ns, tempFilePath).SaveToFile(save);
                }
            }

        }

        public static (ICodegenOutputFile, string?) GenerateRepository(KeyValuePair<string, IOpenApiAny> entity,
            string ns, string tempFilePath)
        {
            string cl = $"{FormatName(entity.Key)}";
            var ctx = new CodegenContext();
            var w = ctx[$"{cl}Repository.cs"];
            w.WriteLine("using Entities;");
            w.WriteLine("using Context;\n");
            w.WithCurlyBraces($"namespace {ns}", () =>
            {
                w.WithCurlyBraces($"public class {cl}Repository : BaseRepository<{cl}Entity>", () =>
                {
                    w.WriteLine($"public {cl}Repository(ApiDbContext context) : base(context){{ }}\n");
                });
            });

            return (w, $"{tempFilePath}/Infrastructure/{ns}/Implements/");
        }

        private static (ICodegenOutputFile, string?) BaseRepositoryGenerator(string tempFilePath)
        {
            var ctx = new CodegenContext();
            var w = ctx[$"BaseRepository.cs"];
            w.WriteLine("using Context;");
            w.WriteLine("using Microsoft.EntityFrameworkCore;\n");

            w.WithCurlyBraces($"namespace {ns}", () =>
            {

                w.WithCurlyBraces($"public class BaseRepository<T> : IBaseRepository<T> where T : class", () =>
                {
                    w.WriteLine("protected ApiDbContext context;");
                    w.WriteLine("protected DbSet<T> dbSet;");

                    w.WithCurlyBraces($"public BaseRepository(ApiDbContext context)", () =>
                    {
                        w.WriteLine($"this.context = context;");
                        w.WriteLine($"dbSet = context.Set<T>();");
                    });

                    w.WithCurlyBraces($"public virtual T GetById(dynamic id)", () =>
                    {
                        w.WriteLine("return dbSet.Find(id);");
                    });

                    w.WithCurlyBraces($"public virtual IQueryable<T> Get()", () =>
                    {
                        w.WriteLine("return dbSet;");
                    });

                    w.WithCurlyBraces($"public virtual T Post(T obj)", () =>
                    {
                        w.WriteLine("var result = dbSet.Add(obj).Entity;");
                        w.WriteLine("context.SaveChanges();");
                        w.WriteLine("return result;");
                    });

                    w.WithCurlyBraces($"public virtual T Put(T obj)", () =>
                    {
                        w.WriteLine("var result = dbSet.Attach(obj).Entity;");
                        w.WriteLine("context.SaveChanges();");
                        w.WriteLine("return result;");
                    });

                    w.WithCurlyBraces($"public virtual T Delete(dynamic id)", () =>
                    {
                        w.WriteLine("var find = dbSet.Find(id);");
                        w.WriteLine("var result = dbSet.Remove(find).Entity;");
                        w.WriteLine("context.SaveChanges();");
                        w.WriteLine("return result;");
                    });

                });
            });

            return (w, $"{tempFilePath}/Infrastructure/{ns}/Implements/");

        }


        private static (ICodegenOutputFile, string?) IBaseRepositoryGenerator(string tempFilePath)
        {
            var ctx = new CodegenContext();
            var w = ctx[$"IBaseRepository.cs"];

            w.WithCurlyBraces($"namespace {ns}", () =>
            {
                w.WithCurlyBraces($"public interface IBaseRepository<T> where T : class", () =>
                {
                    w.WriteLine("T GetById(dynamic id);");
                    w.WriteLine("IQueryable<T> Get();");
                    w.WriteLine("T Post(T obj);");
                    w.WriteLine("T Put(T obj);");
                    w.WriteLine("T Delete(dynamic id);");
                });

            });

            return (w, $"{tempFilePath}/Infrastructure/{ns}/Interfaces/");
        }

    }
}
