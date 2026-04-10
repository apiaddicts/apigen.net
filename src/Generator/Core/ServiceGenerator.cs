using CodegenCS;
using Generator.Utils;
using Humanizer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;
using static Generator.Utils.FileUtils;

namespace Generator.Core
{
    public static class ServiceGenerator
    {
        private static readonly string ns = "Services";

        /// <summary>
        /// A service is generated for each `x-apigen-models` tag defined.
        /// If they do not exist, this process is ignored.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="tempFilePath"></param>
        /// <param name="save"></param>
        public static void Generator(OpenApiDocument doc, string tempFilePath, bool save = true)
        {
            Log.Debug($"Generating ~ Services");
            string projectName = OpenApiUtils.GetProjectName(doc);
            string projectNs = $"{projectName}.Domain.Services";
            IBaseServiceGenerator(tempFilePath, projectName).SaveToFile();
            BaseServiceGenerator(tempFilePath, projectName).SaveToFile();

            var apigenModels = OpenApiUtils.GetApiGenModelsOrDefault(doc);

            if (apigenModels != null)
            {
                foreach (var entity in apigenModels)
                {
                    GenerateService(entity, projectNs, tempFilePath, projectName).SaveToFile(save);
                }
            }
        }

        public static (ICodegenOutputFile, string?) GenerateService(KeyValuePair<string, IOpenApiAny> entity,
            string ns, string tempFilePath, string projectName = "")
        {
            string cl = $"{entity.Key.Pascalize()}";
            var ctx = new CodegenContext();
            var w = ctx[$"{cl}Service.cs"];
            w.WriteLine($"using {projectName}.Infrastructure.Entities;");
            w.WriteLine($"using {projectName}.Infrastructure.Repositories;\n");
            w.WithCurlyBraces($"namespace {ns}", () =>
            {
                w.WithCurlyBraces($"public class {cl}Service : BaseService<{cl}>", () =>
                {
                    w.WriteLine($"private readonly {cl}Repository _{cl.Camelize()}Repository;");

                    w.WithCurlyBraces($"public {cl}Service ({cl}Repository {cl.Camelize()}Repository) : base({cl.Camelize()}Repository)", () =>
                    {
                        w.WriteLine($"_{cl.Camelize()}Repository = {cl.Camelize()}Repository;");
                    });

                });
            });

            return (w, $"{tempFilePath}/src/Domain/{ns}/Implements/");
        }

        /// <summary>
        /// Service pattern from which it extends, defined by apigen.
        /// </summary>
        /// <param name="tempFilePath"></param>
        /// <returns></returns>
        private static (ICodegenOutputFile, string?) BaseServiceGenerator(string tempFilePath, string projectName)
        {
            var ctx = new CodegenContext();
            var w = ctx[$"BaseService.cs"];

            w.WriteLine($$"""
            using Microsoft.EntityFrameworkCore;
            using Microsoft.EntityFrameworkCore.DynamicLinq;
            using {{projectName}}.Infrastructure.Repositories;
            using System.ComponentModel.DataAnnotations;
            using System.Linq.Dynamic.Core;
            using {{projectName}}.Domain.Utils;

            namespace {{projectName}}.Domain.Services
            {
                public class BaseService<T>(IBaseRepository<T> repository) : IBaseService<T> where T : class
                {
                    protected IBaseRepository<T> repository = repository;

                    public virtual IQueryable<T> IQueryableGetById(dynamic id)
                    {
                        var keyProperty = typeof(T).GetProperties().SingleOrDefault(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Length != 0)
                            ?? throw new CustomException($"{typeof(T).Name} Not Key Property", 405, "E405");
                        return repository.Get().AsNoTracking().Where($"{keyProperty.Name} == @0", (object)id);
                    }
                    public virtual async Task<T> GetById(dynamic id, List<string>? select = null, List<string>? exclude = null, List<string>? expand = null)
                    {
                        if (select?.Count != 0 || exclude?.Count != 0 || expand?.Count != 0)
                        {
                            IQueryable<T> result = IQueryableGetById(id);

                            result = result
                            .TryExpand(expand)
                            .TrySelect(select)
                            .TryExclude(exclude);

                            var found = await result.FirstOrDefaultAsync();
                            return found ?? throw new CustomException($"{typeof(T).Name} Not Found with: '{id}'", 404, "E404");
                        }

                        return await SimpleGetById(id);
                    }
                    public virtual async Task<PagedResponse<T>> Get(int init, int limit, bool total, List<string>? orderby = null,
                        List<string>? select = null, List<string>? exclude = null, List<string>? expand = null, string? filter = null)
                    {
                        var result = repository.Get()
                            .AsNoTracking()
                            .TryExpand(expand)
                            .TrySelect(select)
                            .TryExclude(exclude)
                            .TryFilter(filter)
                            .TryOrderBy(orderby);

                        return await result.GetPaged(init, limit, total);
                    }
                    public virtual async Task<T> Post(T obj)
                    {
                        return await repository.Post(obj);
                    }
                    public virtual async Task<T> Put(T obj)
                    {
                        return await repository.Put(obj);
                    }
                    public virtual async Task<T> Delete(dynamic id)
                    {
                        var find = await SimpleGetById(id);
                        return await repository.Delete(find);
                    }
                    public virtual async Task<T> SimpleGetById(dynamic id)
                    {
                        var result = await repository.GetById(id);
                        return result == null ? throw new CustomException($"{typeof(T).Name} Not Found with Id: '{id}'", 404, "E404") : (T)result;
                    }

                }
            }
            
            """);
            return (w, $"{tempFilePath}/src/Domain/{ns}/Implements/");

        }

        private static (ICodegenOutputFile, string?) IBaseServiceGenerator(string tempFilePath, string projectName)
        {
            var ctx = new CodegenContext();
            var w = ctx[$"IBaseService.cs"];

            w.WriteLine($$"""
            using {{projectName}}.Domain.Utils;

            namespace {{projectName}}.Domain.Services
            {
                public interface IBaseService<T> where T : class
                {
                    Task<T> GetById(dynamic id, List<string>? select = null, List<string>? exclude = null, List<string>? expand = null);
                    Task<PagedResponse<T>> Get(int init, int limit, bool total, List<string>? orderby = null, List<string>? select = null, List<string>? exclude = null, List<string>? expand = null, string? filter = null);
                    Task<T> Post(T obj);
                    Task<T> Put(T obj);
                    Task<T> Delete(dynamic id);
                    Task<T> SimpleGetById(dynamic id);
                    IQueryable<T> IQueryableGetById(dynamic id);
                }
            }
            """);
            return (w, $"{tempFilePath}/src/Domain/{ns}/Interfaces/");
        }

    }
}
