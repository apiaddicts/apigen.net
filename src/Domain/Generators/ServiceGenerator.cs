using CodegenCS;
using Domain.Utils;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;
using static Domain.Utils.StringUtils;

namespace Domain.Generators
{
    public static class ServiceGenerator
    {
        private static readonly string ns = "Services";

        public static void Generator(OpenApiDocument doc, string tempFilePath, bool save = true)
        {
            Log.Debug($"Generate Servicies");
            IBaseServiceGenerator(tempFilePath).SaveToFile();
            BaseServiceGenerator(tempFilePath).SaveToFile();
            var apigenModels = (OpenApiObject)doc.Components.Extensions.FirstOrDefault(x => x.Key.Equals("x-apigen-models")).Value;

            if (apigenModels != null)
            {
                foreach (var entity in apigenModels)
                {
                    GenerateService(entity, ns, tempFilePath).SaveToFile(save);
                }
            }

        }

        public static (ICodegenOutputFile, string?) GenerateService(KeyValuePair<string, IOpenApiAny> entity,
            string ns, string tempFilePath)
        {
            string cl = $"{FormatName(entity.Key)}";
            var ctx = new CodegenContext();
            var w = ctx[$"{cl}Service.cs"];
            w.WriteLine("using Entities;");
            w.WriteLine("using Repositories;\n");
            w.WithCurlyBraces($"namespace {ns}", () =>
            {
                w.WithCurlyBraces($"public class {cl}Service : BaseService<{cl}Entity>", () =>
                {
                    w.WriteLine($"private readonly {cl}Repository _{cl.ToCamelCase()}Repository;");

                    w.WithCurlyBraces($"public {cl}Service ({cl}Repository {cl.ToCamelCase()}Repository) : base({cl.ToCamelCase()}Repository)", () =>
                    {
                        w.WriteLine($"_{cl.ToCamelCase()}Repository = {cl.ToCamelCase()}Repository;");
                    });

                });
            });

            return (w, $"{tempFilePath}/Domain/{ns}/Implements/");
        }

        private static (ICodegenOutputFile, string?) BaseServiceGenerator(string tempFilePath)
        {
            var ctx = new CodegenContext();
            var w = ctx[$"BaseService.cs"];
            w.WriteLine("using System.Linq.Dynamic.Core;");
            w.WriteLine("using Microsoft.EntityFrameworkCore;");
            w.WriteLine("using Utils;");
            w.WriteLine("using Models;");
            w.WriteLine("using Repositories;\n");
            w.WithCurlyBraces($"namespace {ns}", () =>
            {

                w.WithCurlyBraces($"public class BaseService<T> : IBaseService<T> where T : class", () =>
                {
                    w.WriteLine("protected IBaseRepository<T> repository;");

                    w.WithCurlyBraces($"public BaseService(IBaseRepository<T> repository)", () =>
                    {
                        w.WriteLine($"this.repository = repository;");
                    });

                    w.WithCurlyBraces($"public virtual T GetById(dynamic id)", () =>
                    {
                        w.WriteLine("return repository.GetById(id);");
                    });

                    w.WithCurlyBraces($"public virtual PagedResponse<T> Get(int init, int limit, bool total, List<string>? select = null, List<string>? expand = null, List<string>? orderby = null, StandardSearchModel? body = null)", () =>
                    {
                        w.WriteLine("var result = repository.Get();");

                        w.WithCurlyBraces($"if (expand != null && expand.Any())", () =>
                        {
                            w.WithCurlyBraces($"foreach(var include in expand)", () =>
                            {
                                w.WriteLine("result = result.Include(include.Format());");
                            });
                        });

                        w.WithCurlyBraces($"if (select != null && select.Any())", () =>
                        {
                            w.WriteLine("//result = result.Select<T>($\"new {{ { string.Join(\", \", select.ToArray())} }}\");");
                            w.WriteLine("ApigenSelect.Apply(result, select);");

                        });

                        w.WithCurlyBraces($"if (body != null && body.Filter != null)", () =>
                        {
                            w.WriteLine("string query = \"\";");

                            w.WithCurlyBraces($"if (body.Filter.Values != null)", () =>
                            {
                                w.WriteLine("query = StandardSearchModel.QuerySearchBuilder(body.Filter.Operation, body.Filter.Values);");
                            });
                            w.WriteLine("result = result.Where(query);");
                        });

                        w.WithCurlyBraces($"if (orderby != null && orderby.Any())", () =>
                        {
                            w.WriteLine("result = result.OrderBy(string.Join(\", \", orderby.ToArray()));");
                        });

                        w.WriteLine("return result.GetPaged(init, limit, total);");
                    });

                    w.WithCurlyBraces($"public virtual T Post(T obj)", () =>
                    {
                        w.WriteLine("return repository.Post(obj);");
                    });

                    w.WithCurlyBraces($"public virtual T Put(T obj)", () =>
                    {
                        w.WriteLine("return repository.Put(obj);");
                    });

                    w.WithCurlyBraces($"public virtual T Delete(dynamic id)", () =>
                    {
                        w.WriteLine("return repository.Delete(id);");
                    });

                });
            });

            return (w, $"{tempFilePath}/Domain/{ns}/Implements/");

        }


        private static (ICodegenOutputFile, string?) IBaseServiceGenerator(string tempFilePath)
        {
            var ctx = new CodegenContext();
            var w = ctx[$"IBaseService.cs"];
            w.WriteLine("using Utils;");
            w.WriteLine("using Models;");
            w.WithCurlyBraces($"namespace {ns}", () =>
            {
                w.WithCurlyBraces($"public interface IBaseService<T> where T : class", () =>
                {
                    w.WriteLine("T GetById(dynamic id);");
                    w.WriteLine("PagedResponse<T> Get(int init, int limit, bool total, List<string>? select = null, List<string>? expand = null, List<string>? orderby = null, StandardSearchModel? body = null);");
                    w.WriteLine("T Post(T obj);");
                    w.WriteLine("T Put(T obj);");
                    w.WriteLine("T Delete(dynamic id);");
                });

            });

            return (w, $"{tempFilePath}/Domain/{ns}/Interfaces/");
        }

    }
}
