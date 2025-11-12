using CodegenCS;
using Generator.Utils;
using Humanizer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;
using static Generator.Utils.FileUtils;

namespace Generator.Core
{
    public static class RepositoryGenerator
    {
        private static readonly string ns = "Repositories";

        /// <summary>
        /// A repository is generated for each `x-apigen-models` tag defined.
        /// If they do not exist, this process is ignored.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="tempFilePath"></param>
        /// <param name="save"></param>
        public static void Generator(OpenApiDocument doc, string tempFilePath, bool save = true)
        {

            Log.Debug($"Generating ~ Repositories");
            IBaseRepositoryGenerator(tempFilePath).SaveToFile();
            BaseRepositoryGenerator(tempFilePath).SaveToFile();
            var apigenModels = OpenApiUtils.GetApiGenModelsOrDefault(doc);

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
            string cl = $"{entity.Key.Pascalize()}";
            var ctx = new CodegenContext();
            var w = ctx[$"{cl}Repository.cs"];
            w.WriteLine("using Entities;");
            w.WriteLine("using Context;\n");
            w.WithCurlyBraces($"namespace {ns}", () =>
            {
                w.WithCurlyBraces($"public class {cl}Repository : BaseRepository<{cl}>", () =>
                {
                    w.WriteLine($"public {cl}Repository(ApiDbContext context) : base(context){{ }}\n");
                });
            });

            return (w, $"{tempFilePath}/src/Infrastructure/{ns}/Implements/");
        }

        /// <summary>
        /// Repository pattern from which it extends, defined by apigen.
        /// </summary>
        /// <param name="tempFilePath"></param>
        private static (ICodegenOutputFile, string?) BaseRepositoryGenerator(string tempFilePath)
        {
            var ctx = new CodegenContext();
            var w = ctx[$"BaseRepository.cs"];

            w.WriteLine($$"""
            using Context;
            using Microsoft.EntityFrameworkCore;

            namespace Repositories
            {
                public class BaseRepository<T> : IBaseRepository<T> where T : class
                {
                    protected ApiDbContext context;
                    protected DbSet<T> dbSet;
                    public BaseRepository(ApiDbContext context)
                    {
                        this.context = context;
                        dbSet = context.Set<T>();
                    }
                    public virtual IQueryable<T> Get()
                    {
                        return dbSet;
                    }
                    public virtual async Task<T> Post(T obj)
                    {
                        var result = await dbSet.AddAsync(obj);
                        await context.SaveChangesAsync();
                        return result.Entity;
                    }
                    public virtual async Task<T> Put(T obj)
                    {
                        var result = dbSet.Attach(obj);
                        context.Entry(obj).State = EntityState.Modified;
                        await context.SaveChangesAsync();
                        return result.Entity;
                    }
                    public virtual async Task<T> Delete(T obj)
                    {
                        var result = dbSet.Remove(obj);
                        context.Entry(obj).State = EntityState.Deleted;
                        await context.SaveChangesAsync();
                        return result.Entity;
                    }
                    public virtual async Task<T> GetById(dynamic id)
                    {
                        return await dbSet.FindAsync(id);
                    }
                }
            }
            """);

            return (w, $"{tempFilePath}/src/Infrastructure/{ns}/Implements/");

        }

        private static (ICodegenOutputFile, string?) IBaseRepositoryGenerator(string tempFilePath)
        {
            var ctx = new CodegenContext();
            var w = ctx[$"IBaseRepository.cs"];

            w.WriteLine($$"""
            namespace Repositories
            {
                public interface IBaseRepository<T> where T : class
                {
                    IQueryable<T> Get();
                    Task<T> Post(T obj);
                    Task<T> Put(T obj);
                    Task<T> Delete(T obj);
                    Task<T> GetById(dynamic id);
                }
            }
            """);

            return (w, $"{tempFilePath}/src/Infrastructure/{ns}/Interfaces/");
        }

    }
}
