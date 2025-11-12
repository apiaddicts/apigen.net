using CodegenCS;
using Generator.Utils;
using Humanizer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using static Generator.Utils.FileUtils;
using static Generator.Utils.OpenApiUtils;

namespace Generator.Core
{
    public static class TestGenerator
    {
        private static readonly string defaultTag = "default";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="tempFilePath"></param>
        public static void Generator(OpenApiDocument doc, string tempFilePath)
        {
            Log.Debug($"Generating ~ Tests");
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
            var className = GetClassName(tag);
            var ctx = new CodegenContext();
            var writer = ctx[$"{className}ControllerTest.cs"];

            if (tag.Entity == null) return (writer, null);

            var entity = GetEntityName(tag);
            WriteUsings(writer);

            writer.WithCurlyBraces($"namespace Test.Api.Helpers", () =>
            {
                writer.WithCurlyBraces($"public class {className}ControllerTest", () =>
                {
                    DefineClassMembers(writer, className, entity);
                    InitializeClassMembersInConstructor(writer, className, entity);
                    GenerateTestMethods(writer, doc, tag, entity);
                });
            });

            return (writer, $"{tempFilePath}/test/UnitTest/Api/Controllers/");
        }

        private static string GetClassName((OpenApiTag Tag, OpenApiString? Entity) tag)
        {
            return tag.Tag.Name.Pascalize();
        }

        private static string GetEntityName((OpenApiTag Tag, OpenApiString? Entity) tag)
        {
            return tag.Entity!.Value.Pascalize();
        }

        private static void WriteUsings(ICodegenOutputFile writer)
        {
            writer.WriteLine("using AutoMapper;");
            writer.WriteLine("using Context;");
            writer.WriteLine("using Models;");
            writer.WriteLine("using Controllers;");
            writer.WriteLine("using Microsoft.AspNetCore.Mvc;");
            writer.WriteLine("using Moq;");
            writer.WriteLine("using Repositories;");
            writer.WriteLine("using Services;");
            writer.WriteLine("using Xunit;\n");
        }

        private static void DefineClassMembers(ICodegenOutputFile writer, string className, string entity)
        {
            writer.WriteLine($"private readonly {className}Controller controller;");
            writer.WriteLine($"private readonly Mock<{entity}Service> service;");
            writer.WriteLine($"private readonly Mock<{entity}Repository> repository;");
            writer.WriteLine($"private readonly Mock<ApiDbContext> context;");
            writer.WriteLine($"private readonly Mock<IMapper> mapper;");
        }

        private static void InitializeClassMembersInConstructor(ICodegenOutputFile writer, string className, string entity)
        {
            writer.WithCurlyBraces($"public {className}ControllerTest()", () =>
            {
                writer.WriteLine($"mapper = new Mock<IMapper>();");
                writer.WriteLine($"context = new Mock<ApiDbContext>();");
                writer.WriteLine($"repository = new Mock<{entity}Repository>(context.Object);");
                writer.WriteLine($"service = new Mock<{entity}Service>(repository.Object);");
                writer.WriteLine($"controller = new {className}Controller(mapper.Object, service.Object);");
            });
        }

        private static void GenerateTestMethods(ICodegenOutputFile writer, OpenApiDocument doc, (OpenApiTag Tag, OpenApiString? Entity) tag, string entity)
        {
            foreach (var path in doc.Paths)
            {
                foreach (var operation in path.Value.Operations)
                {
                    if (operation.Value.Tags.Contains(tag.Tag) || tag.Tag.Name.Equals(defaultTag))
                    {
                        writer.WriteLine("[Fact]");
                        var functionName = operation.Value.OperationId.Pascalize();
                        writer.WithCurlyBraces($"public async Task {functionName}Test()", () =>
                        {
                            var resultType = DetermineResultType(operation);
                            SetupMocksForOperation(writer, operation, entity, resultType);
                            ExecuteTestMethod(writer, functionName, resultType, operation);
                        });
                    }
                }
            }
        }

        private static string DetermineResultType(KeyValuePair<OperationType, OpenApiOperation> operation)
        {
            return operation.Value.Responses.FirstOrDefault().Key.Equals("204") ? "StatusCodeResult" : "ObjectResult";
        }

        private static void SetupMocksForOperation(ICodegenOutputFile writer, KeyValuePair<OperationType, OpenApiOperation> operation, string entity, string resultType)
        {
            if (resultType == "StatusCodeResult")
            {
                var op = operation.Key.Equals(OperationType.Patch) ? OperationType.Put : operation.Key;
                writer.WriteLine($"service.Setup(x => x.{op}(It.IsAny<Entities.{entity}>())).Returns(It.IsAny<Entities.{entity}>());");
                writer.WriteLine($"repository.Setup(x => x.{op}(It.IsAny<Entities.{entity}>())).Returns(It.IsAny<Entities.{entity}>());");
            }
        }

        private static void ExecuteTestMethod(ICodegenOutputFile writer, string functionName, string resultType, KeyValuePair<OperationType, OpenApiOperation> operation)
        {
            writer.WriteLine($"var result = ({resultType}) await controller.{functionName}({AddIsAny(operation.Value)});");
            writer.WriteLine($"Assert.IsType<{resultType}>(result);");
            writer.WriteLine($"Assert.Equal({operation.Value.Responses.FirstOrDefault().Key}, result?.StatusCode);");
        }

        private static string AddIsAny(OpenApiOperation operation)
        {
            StringBuilder builder = new();
            AppendParameters(builder, operation);
            AppendRequestBody(builder, operation);
            return builder.ToString();
        }

        private static void AppendParameters(StringBuilder builder, OpenApiOperation operation)
        {
            for (int i = 0; i < operation.Parameters.Count; i++)
            {
                var param = operation.Parameters[i];
                builder.Append(AddSchema(param.Schema));

                if (i != operation.Parameters.Count - 1)
                    builder.Append(", ");
            }
        }

        private static void AppendRequestBody(StringBuilder builder, OpenApiOperation operation)
        {
            if (operation.RequestBody != null)
            {
                if (builder.Length > 0)
                    builder.Append(", ");

                var content = operation.RequestBody.Content;
                AppendContentBasedOnMediaType(builder, content);
            }
        }

        private static void AppendContentBasedOnMediaType(StringBuilder builder, IDictionary<string, OpenApiMediaType> content)
        {
            if (content.TryGetValue("multipart/form-data", out _))
                builder.Append("It.IsAny<IFormFile>()");
            else if (content.TryGetValue("application/json", out var appJson))
                AppendJsonContent(builder, appJson.Schema);
            else if (content.TryGetValue("application/x-www-form-urlencoded", out var formUrlEncoded))
                AppendFormUrlEncodedContent(builder, formUrlEncoded.Schema);
        }

        private static void AppendJsonContent(StringBuilder builder, OpenApiSchema schema)
        {
            if (schema.Reference != null)
                builder.Append($"It.IsAny<{schema.Reference.Id.Pascalize()}>()");
        }

        private static void AppendFormUrlEncodedContent(StringBuilder builder, OpenApiSchema schema)
        {
            if (schema.Properties != null)
            {
                foreach (var (key, value) in schema.Properties)
                {
                    builder.Append($"It.IsAny<{value.Type}>()");

                    if (!key.Equals(schema.Properties.Keys.Last()))
                        builder.Append(", ");
                }
            }
        }

        private static string AddSchema(OpenApiSchema Schema)
        {

            if (Schema.Format != null && Schema.Format.Contains("int"))
                return "It.IsAny<int>()";

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

            w.WriteLine($$"""
            using Context;
            using Moq;
            using Repositories;
            using Services;
            using Xunit;
            using MockQueryable;

            namespace Test.Domain.Services
            {
                public record Pojo(int Id, string Name, string Surname);
                public class BaseServiceTest
                {
                    private readonly BaseService<dynamic> service;
                    private readonly Mock<ApiDbContext> context;
                    private readonly Mock<BaseRepository<dynamic>> repository;
                    private Pojo obj;

                    public BaseServiceTest()
                    {
                        context = new Mock<ApiDbContext>();
                        repository = new Mock<BaseRepository<dynamic>>(context.Object);
                        service = new BaseService<dynamic>(repository.Object);
                        obj = new Pojo (1, "name", "surname" );
                    }

                    [Fact]
                    public async Task Get()
                    {
                        var list = new List<dynamic> { obj, obj, obj, obj };
                        var mock = list.BuildMock();
                        repository.Setup(x => x.Get()).Returns(mock);
                        var result = await service.Get(1, 10, true, It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>());
                        Assert.Equal(4, result.RowCount);
                    }
                    [Fact]
                    public void Post()
                    {
                        repository.Setup(x => x.Post(obj)).ReturnsAsync(obj);
                        var result = service.Post(obj);
                        Assert.NotNull(result);
                    }
                    [Fact]
                    public void Put()
                    {
                        repository.Setup(x => x.Put(obj)).ReturnsAsync(obj);
                        var result = service.Put(obj);
                        Assert.NotNull(result);
                    }
                    [Fact]
                    public void Delete()
                    {
                        repository.Setup(x => x.GetById(obj.Id)).ReturnsAsync(obj);
                        repository.Setup(x => x.Delete(It.IsAny<object>())).ReturnsAsync(obj);
                        var result = service.Delete(obj.Id);
                        Assert.NotNull(result);
                    }
                    [Fact]
                    public void GetById()
                    {
                        repository.Setup(x => x.GetById(obj.Id)).ReturnsAsync(obj);
                        var result = service.GetById(obj.Id);
                        Assert.NotNull(result);
                    }
                }
            }
            """);

            return (w, $"{tempFilePath}/test/UnitTest/Domain/Services/");
        }

        private static (ICodegenOutputFile, string?) GenerateHttpResponseExceptionFilterTest(string tempFilePath)
        {
            var ctx = new CodegenContext();
            var w = ctx[$"HttpResponseExceptionFilterTest.cs"];

            w.WriteLine($$"""
            using Helpers;
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Mvc;
            using Microsoft.AspNetCore.Mvc.Abstractions;
            using Microsoft.AspNetCore.Mvc.Filters;
            using Microsoft.AspNetCore.Mvc.ModelBinding;
            using Microsoft.AspNetCore.Routing;
            using Moq;
            using Xunit;

            namespace Test.Api.Helpers
            {
                public class HttpResponseExceptionFilterTest
                {
                    [Fact]
                    public void FilterBadRequest()
                    {
                        HttpResponseExceptionFilter addHeaderAttribute = new HttpResponseExceptionFilter();
                        var modelState = new ModelStateDictionary();
                        modelState.AddModelError("name", "invalid");
                        var actionContext = new ActionContext(Mock.Of<HttpContext>(), Mock.Of<RouteData>(), Mock.Of<ActionDescriptor>(), modelState);
                        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), Mock.Of<Controller>());
                        addHeaderAttribute.OnActionExecuting(actionExecutingContext);
                        var result = (ObjectResult?)actionExecutingContext.Result;
                        Assert.IsType<ObjectResult>(result);
                        Assert.Equal(400, result?.StatusCode);
                    }
                    [Fact]
                    public void FilterException()
                    {
                        HttpResponseExceptionFilter addHeaderAttribute = new HttpResponseExceptionFilter();
                        var actionContext = new ActionContext(Mock.Of<HttpContext>(), Mock.Of<RouteData>(), Mock.Of<ActionDescriptor>(), new ModelStateDictionary());
                        var actionExecutedContext = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), Mock.Of<Controller>());
                        actionExecutedContext.Exception = new System.Exception();
                        addHeaderAttribute.OnActionExecuted(actionExecutedContext);
                        var result = (ObjectResult?)actionExecutedContext.Result;
                        Assert.IsType<ObjectResult>(result);
                        Assert.Equal(500, result?.StatusCode);
                    }
                }
            }
            """);

            return (w, $"{tempFilePath}/test/UnitTest/Domain/Helpers/");
        }

    }
}
