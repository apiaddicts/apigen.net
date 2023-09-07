using CodegenCS;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using static Domain.Utils.FileUtils;
using static Domain.Utils.OpenApiUtils;
using static Domain.Utils.StringUtils;

namespace Domain.Generators
{
    public static class TestGenerator
    {
        private static readonly string defaultTag = "default";

        public static void Generator(OpenApiDocument doc, string tempFilePath)
        {
            Log.Debug($"Generate Controllers");
            var tags = TagsByDocPath(doc);

            foreach (var tag in tags)
            {
                GenerateControllerTest(tag, doc, tempFilePath).SaveToFile();
            }

            GenerateBaseServiceTest(tempFilePath).SaveToFile();
            GenerateHttpResponseExceptionFilterTest(tempFilePath).SaveToFile();
        }


        private static (ICodegenOutputFile, string?) GenerateControllerTest((OpenApiTag Tag, OpenApiString? Entity) tag, OpenApiDocument doc, string tempFilePath)
        {


            string cl = $"{FormatName(tag.Tag.Name)}";
            var ctx = new CodegenContext();
            var w = ctx[$"{cl}ControllerTest.cs"];

            if (tag.Entity == null) return (w, null);

            var entity = $"{FormatName(tag.Entity!.Value)}";

            w.WriteLine("using AutoMapper;");
            w.WriteLine("using Context;");
            w.WriteLine("using Models;");
            w.WriteLine("using Controllers;");
            w.WriteLine("using Microsoft.AspNetCore.Mvc;");
            w.WriteLine("using Moq;");
            w.WriteLine("using Repositories;");
            w.WriteLine("using Services;");
            w.WriteLine("using Xunit;\n");

            w.WithCurlyBraces($"namespace Test.Api.Helpers", () =>
            {
                w.WithCurlyBraces($"public class {cl}ControllerTest", () =>
                {
                    w.WriteLine($"private readonly {cl}Controller controller;");
                    w.WriteLine($"private readonly Mock<{entity}Service> service;");
                    w.WriteLine($"private readonly Mock<{entity}Repository> repository;");
                    w.WriteLine($"private readonly Mock<ApiDbContext> context;");
                    w.WriteLine($"private readonly Mock<IMapper> mapper;");


                    w.WithCurlyBraces($"public {cl}ControllerTest()", () =>
                    {
                        w.WriteLine($"mapper = new Mock<IMapper>();");
                        w.WriteLine($"context = new Mock<ApiDbContext>();");
                        w.WriteLine($"repository = new Mock<{entity}Repository>(context.Object);");
                        w.WriteLine($"service = new Mock<{entity}Service>(repository.Object);");
                        w.WriteLine($"controller = new {cl}Controller(mapper.Object, service.Object);");
                    });

                    foreach (var path in doc.Paths)
                    {
                        foreach (var operation in path.Value.Operations)
                        {

                            if (operation.Value.Tags.Contains(tag.Tag) || tag.Tag.Name.Equals(defaultTag))
                            {
                                w.WriteLine("[Fact]");
                                var function = operation.Value.OperationId.ToPascalCase();
                                w.WithCurlyBraces($"public void {function}Test()", () =>
                                {
                                    w.WriteLine($"var result = (ObjectResult) controller.{function}({AddIsAny(operation.Value)});");
                                    w.WriteLine($"Assert.IsType<ObjectResult>(result);");
                                    w.WriteLine($"Assert.Equal({operation.Value.Responses.FirstOrDefault().Key}, result?.StatusCode);");
                                });
                            }

                        }
                    }
                });
            });

            return (w, $"{tempFilePath}/Test/Api/Controllers/");
        }

        private static string AddIsAny(OpenApiOperation operations)
        {
            StringBuilder builder = new();

            for (int i = 0; i < operations.Parameters.Count; i++)
            {
                var param = operations.Parameters[i];
                builder.Append($"{AddSchema(param.Schema)}");

                if (operations.Parameters.Any() && i != operations.Parameters.Count - 1)
                    builder.Append(", ");
            }

            if (operations.RequestBody != null)
            {
                if (builder.Length != 0)
                    builder.Append(", ");

                var content = operations.RequestBody.Content;
                if (content.ContainsKey("multipart/form-data"))
                    builder.Append($"It.IsAny<IFormFile>()");
                else if (content.ContainsKey("application/json"))
                {
                    var schema = content["application/json"].Schema;
                    if (schema.Reference != null)
                        builder.Append($"It.IsAny<{FormatName(schema.Reference.Id)}>()");
                }
                else if (content.ContainsKey("application/x-www-form-urlencoded"))
                {
                    var schema = content["application/x-www-form-urlencoded"].Schema;

                    if (schema.Properties != null)
                    {
                        for (int i = 0; i < schema.Properties.Count; i++)
                        {
                            var properties = schema.Properties.ElementAt(i);
                            builder.Append($"It.IsAny<{properties.Value.Type}>()");

                            if (schema.Properties.Any() && i != schema.Properties.Count - 1)
                                builder.Append(", ");
                        }
                    }

                }

            }

            return builder.ToString();
        }


        private static string AddSchema(OpenApiSchema Schema)
        {

            if (Schema.Format != null)
            {
                if (Schema.Format.Contains("int"))
                    return "It.IsAny<int>()";
            }

            if (Schema.Type != null)
            {
                if (Schema.Type.Contains("string"))
                    return "It.IsAny<string>()";
                if (Schema.Type.Contains("array"))
                    return "It.IsAny<List<string>>()";
                if (Schema.Type.Contains("boolean"))
                    return "It.IsAny<bool>()";
            }

            return "It.IsAny<object>()";
        }

        private static (ICodegenOutputFile, string?) GenerateBaseServiceTest(string tempFilePath)
        {
            var ctx = new CodegenContext();
            var w = ctx[$"BaseServiceTest.cs"];
            w.WriteLine("using Context;");
            w.WriteLine("using Moq;");
            w.WriteLine("using Repositories;");
            w.WriteLine("using Services;");
            w.WriteLine("using Xunit;\n");

            w.WithCurlyBraces($"namespace Test.Domain.Services", () =>
            {
                w.WithCurlyBraces($"public class BaseServiceTest", () =>
                {

                    w.WriteLine("private readonly BaseService<dynamic> service;");
                    w.WriteLine("private readonly Mock<ApiDbContext> context;");
                    w.WriteLine("private readonly Mock<BaseRepository<dynamic>> repository;");
                    w.WriteLine("private readonly object obj;");
                    w.WithCurlyBraces($"public BaseServiceTest()", () =>
                    {
                        w.WriteLine("context = new Mock<ApiDbContext>();");
                        w.WriteLine("repository = new Mock<BaseRepository<dynamic>>(context.Object);");
                        w.WriteLine("service = new BaseService<dynamic>(repository.Object);");
                        w.WriteLine("obj = new { id = 1, name = \"name\", surname = \"surname\" };");
                    });

                    w.WriteLine("[Fact]");
                    w.WithCurlyBraces($"public void Get()", () =>
                    {
                        w.WriteLine("var list = new List<dynamic> { obj, obj, obj, obj };");
                        w.WriteLine("repository.Setup(x => x.Get()).Returns(list.AsQueryable());");
                        w.WriteLine("var result = service.Get(1, 10, true, It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>());");
                        w.WriteLine("Assert.Equal(4, result.RowCount);");
                    });
                    w.WriteLine("[Fact]");
                    w.WithCurlyBraces($"public void GetById()", () =>
                    {
                        w.WriteLine("var list = new List<dynamic> { obj, obj, obj, obj };");
                        w.WriteLine("repository.Setup(x => x.GetById(1)).Returns(obj);");
                        w.WriteLine("var result = service.GetById(1);");
                        w.WriteLine("Assert.Equal(1, result.id);");
                    });
                    w.WriteLine("[Fact]");
                    w.WithCurlyBraces($"public void Post()", () =>
                    {
                        w.WriteLine("repository.Setup(x => x.Post(obj)).Returns(obj);");
                        w.WriteLine("var result = service.Post(obj);");
                        w.WriteLine("Assert.NotNull(result);");
                    });
                    w.WriteLine("[Fact]");
                    w.WithCurlyBraces($"public void Put()", () =>
                    {
                        w.WriteLine("repository.Setup(x => x.Put(obj)).Returns(obj);");
                        w.WriteLine("var result = service.Put(obj);");
                        w.WriteLine("Assert.NotNull(result);");
                    });
                    w.WriteLine("[Fact]");
                    w.WithCurlyBraces($"public void Delete()", () =>
                    {
                        w.WriteLine("repository.Setup(x => x.Delete(obj)).Returns(obj);");
                        w.WriteLine("var result = service.Delete(obj);");
                        w.WriteLine("Assert.NotNull(result);");
                    });

                });
            });

            return (w, $"{tempFilePath}/Test/Domain/Services/");
        }

        private static (ICodegenOutputFile, string?) GenerateHttpResponseExceptionFilterTest(string tempFilePath)
        {
            var ctx = new CodegenContext();
            var w = ctx[$"HttpResponseExceptionFilterTest.cs"];
            w.WriteLine("using Helpers;");
            w.WriteLine("using Microsoft.AspNetCore.Http;");
            w.WriteLine("using Microsoft.AspNetCore.Mvc;");
            w.WriteLine("using Microsoft.AspNetCore.Mvc.Abstractions;");
            w.WriteLine("using Microsoft.AspNetCore.Mvc.Filters;");
            w.WriteLine("using Microsoft.AspNetCore.Mvc.ModelBinding;");
            w.WriteLine("using Microsoft.AspNetCore.Routing;");
            w.WriteLine("using Moq;");
            w.WriteLine("using System.Collections.Generic;");
            w.WriteLine("using Xunit;\n");

            w.WithCurlyBraces($"namespace Test.Api.Helpers", () =>
            {
                w.WithCurlyBraces($"public class HttpResponseExceptionFilterTest", () =>
                {

                    w.WriteLine("[Fact]");
                    w.WithCurlyBraces($"public void FilterBadRequest()", () =>
                    {
                        w.WriteLine("HttpResponseExceptionFilter addHeaderAttribute = new HttpResponseExceptionFilter();");
                        w.WriteLine("var modelState = new ModelStateDictionary();");
                        w.WriteLine("modelState.AddModelError(\"name\", \"invalid\");");
                        w.WriteLine("var actionContext = new ActionContext(Mock.Of<HttpContext>(), Mock.Of<RouteData>(), Mock.Of<ActionDescriptor>(), modelState);");
                        w.WriteLine("var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), Mock.Of<Controller>());");
                        w.WriteLine("addHeaderAttribute.OnActionExecuting(actionExecutingContext);");
                        w.WriteLine("var result = (ObjectResult?)actionExecutingContext.Result;");
                        w.WriteLine("Assert.IsType<ObjectResult>(result);");
                        w.WriteLine("Assert.Equal(400, result?.StatusCode);");

                    });
                    w.WriteLine("[Fact]");
                    w.WithCurlyBraces($"public void FilterException()", () =>
                    {
                        w.WriteLine("HttpResponseExceptionFilter addHeaderAttribute = new HttpResponseExceptionFilter();");
                        w.WriteLine("var actionContext = new ActionContext(Mock.Of<HttpContext>(), Mock.Of<RouteData>(), Mock.Of<ActionDescriptor>(), new ModelStateDictionary());");
                        w.WriteLine("var actionExecutedContext = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), Mock.Of<Controller>());");
                        w.WriteLine("actionExecutedContext.Exception = new System.Exception();");
                        w.WriteLine("addHeaderAttribute.OnActionExecuted(actionExecutedContext);");
                        w.WriteLine("var result = (ObjectResult?)actionExecutedContext.Result;");
                        w.WriteLine("Assert.IsType<ObjectResult>(result);");
                        w.WriteLine("Assert.Equal(500, result?.StatusCode);");
                    });


                });
            });

            return (w, $"{tempFilePath}/Test/Domain/Helpers/");
        }

    }
}
