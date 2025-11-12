using CodegenCS;
using Generator.Utils;
using Humanizer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using static Generator.Utils.FileUtils;
using static Generator.Utils.OpenApiUtils;
using static Generator.Utils.StringUtils;

namespace Generator.Core
{
    public static class ControllersGenerator
    {
        private static readonly string defaultTag = "default";

        public static void Generator(OpenApiDocument doc, string tempFilePath)
        {
            Log.Debug($"Generating ~ Controllers");
            var tags = TagsByDocPath(doc);
            string ns = "Controllers";

            foreach (var tag in tags)
            {
                GenerateControllerForTag(tag, ns, doc, tempFilePath).SaveToFile();
            }

            if (tags.Count == 0)
            {
                GenerateControllerForTag((new OpenApiTag() { Name = defaultTag }, null), ns, doc, tempFilePath).SaveToFile();
            }
        }

        private static (ICodegenOutputFile, string?) GenerateControllerForTag((OpenApiTag Tag, OpenApiString? Entity) tag, string ns, OpenApiDocument doc, string tempFilePath)
        {
            string className = $"{tag.Tag.Name.Pascalize()}Controller";
            var context = new CodegenContext();
            var writer = context[$"{className}.cs"];
            var servicesAlreadyInjected = new List<string>();

            DefineControllerUsingStatements(writer);
            DefineControllerNamespace(writer, ns, className, doc, tag, servicesAlreadyInjected);

            return (writer, $"{tempFilePath}/src/Api/{ns}/");
        }

        private static void DefineControllerUsingStatements(ICodegenOutputFile writer)
        {
            writer.WriteLine("using Microsoft.AspNetCore.Mvc;");
            writer.WriteLine("using AutoMapper;");
            writer.WriteLine();
        }

        private static void DefineControllerNamespace(ICodegenOutputFile writer, string ns, string className, OpenApiDocument doc, (OpenApiTag Tag, OpenApiString? Entity) tag, List<string> servicesAlreadyInjected)
        {
            writer.WithCurlyBraces($"namespace {ns}", () =>
            {
                DefineControllerClass(writer, className, doc, tag, servicesAlreadyInjected);
            });
        }

        private static void DefineControllerClass(ICodegenOutputFile writer, string className, OpenApiDocument doc, (OpenApiTag Tag, OpenApiString? Entity) tag, List<string> servicesAlreadyInjected)
        {
            writer.WriteLine("[ApiController]");
            writer.WriteLine("[Route(\"[controller]\")]");
            writer.WriteLine("[Produces(\"application/json\")]");

            writer.WithCurlyBraces($"public class {className} : ControllerBase", () =>
            {
                AddControllerConstructor(writer, className, doc, tag, servicesAlreadyInjected);
                AddControllerEndpoints(writer, doc, tag, servicesAlreadyInjected);
            });
        }

        private static void AddControllerConstructor(ICodegenOutputFile writer, string className, OpenApiDocument doc, (OpenApiTag Tag, OpenApiString? Entity) tag, List<string> servicesAlreadyInjected)
        {
            if (doc.Paths!=null)
            {
                foreach (var path in doc.Paths)
                {
                    var findTag = path.Value.Operations.Values.FirstOrDefault(x => x.Tags.Contains(tag.Tag));
                    if (findTag != null)
                    {
                        var entity = ReadModelInExtensions(path, servicesAlreadyInjected);
                        if (entity != null)
                            writer.WriteLine($"private readonly Services.{entity}Service _{entity.Camelize()}Service;");
                    }
                }

                writer.WriteLine("private readonly IMapper _mapper;");
                writer.Write($"public {className}(IMapper mapper");
                foreach (var service in servicesAlreadyInjected)
                {
                    writer.Write($", Services.{service}Service {service.Camelize()}Service");
                }
                writer.WriteLine(")");
                writer.WriteLine("{");
                writer.WriteLine("\t_mapper = mapper;");
                foreach (var service in servicesAlreadyInjected)
                {
                    writer.WriteLine($"\t_{service.Camelize()}Service = {service.Camelize()}Service;");
                }
                writer.WriteLine("}");
            }
        }

        private static void AddControllerEndpoints(ICodegenOutputFile writer, OpenApiDocument doc, (OpenApiTag Tag, OpenApiString? Entity) tag, List<string> servicesAlreadyInjected)
        {
            if (doc.Paths != null)
            {
                foreach (var path in doc.Paths)
                {
                    foreach (var operation in path.Value.Operations)
                    {
                        if (operation.Value.Tags.Contains(tag.Tag) || tag.Tag.Name.Equals(defaultTag))
                        {
                            DefineEndpointMethod(writer, path.Key, operation, servicesAlreadyInjected);
                        }
                    }
                }
            }

        }

        private static void DefineEndpointMethod(ICodegenOutputFile writer, string pathKey, KeyValuePair<OperationType, OpenApiOperation> operation, List<string> servicesAlreadyInjected)
        {
            writer.WriteLine($"[Http{operation.Key.ToString().Pascalize()}(\"{pathKey}\")]");
            writer.WithCurlyBraces($"public async Task<IActionResult> {operation.Value.OperationId.Pascalize()}({AddOperations(operation.Value)})", () =>
            {
                if (servicesAlreadyInjected.Count != 0)
                {
                    var id = operation.Value.Parameters?.FirstOrDefault(x => x.In == ParameterLocation.Path)?.Name.Camelize();
                    AddLogic(operation, servicesAlreadyInjected[0], writer, id);
                }
                else
                {
                    writer.WriteLine($"return StatusCode({operation.Value.Responses.FirstOrDefault().Key}, new NotImplementedException());");
                }
            });
            writer.WriteLine();
        }

        private static string? ReadModelInExtensions(KeyValuePair<string, OpenApiPathItem> path, List<string> servicesAlreadyInjected)
        {
            var extensions = (OpenApiObject)path.Value.Extensions.FirstOrDefault(x => x.Key.Equals("x-apigen-binding")).Value;
            if (extensions != null)
            {
                var entity = (OpenApiString)extensions.FirstOrDefault(x => x.Key.Equals("model")).Value;
                if (entity != null && !servicesAlreadyInjected.Contains(entity.Value))
                {
                    servicesAlreadyInjected.Add(entity.Value);
                    return entity.Value;
                }
            }
            return null;
        }

        private static string AddOperations(OpenApiOperation operation)
        {
            StringBuilder builder = new();
            foreach (var param in operation.Parameters)
            {
                string paramAnnotation = param.In switch
                {
                    ParameterLocation.Path => $"[FromRoute(Name = \"{param.Name}\")] ",
                    ParameterLocation.Query => $"[FromQuery(Name = \"{param.Name}\")] ",
                    ParameterLocation.Header => $"[FromHeader(Name = \"{param.Name}\")] ",
                    _ => ""
                };
                builder.Append($"{paramAnnotation}{AddSchema(param.Schema)} {param.Name.CleanString().Camelize()}");
                if (param != operation.Parameters[operation.Parameters.Count - 1])
                    builder.Append(", ");
            }

            if (operation.RequestBody != null)
            {
                if (builder.Length > 0)
                    builder.Append(", ");

                builder.Append(DescribeRequestBody(operation.RequestBody));
            }
            return builder.ToString();
        }

        private static string DescribeRequestBody(OpenApiRequestBody requestBody)
        {
            var content = requestBody.Content;

            if (content.ContainsKey("multipart/form-data"))
            {
                return "IFormFile file";
            }
            else if (content.TryGetValue("application/json", out var appJson))
            {
                var schema = appJson.Schema;
                if (schema != null && schema.Reference != null)
                    return $"[FromBody] Models.{schema.Reference.Id.Pascalize()} body";
                else
                    return $"[FromBody] dynamic body";
            }
            else if (content.TryGetValue("application/x-www-form-urlencoded", out var formUrlEncode))
            {
                var schema = formUrlEncode.Schema;
                if (schema != null && schema.Properties.Any())
                {
                    return string.Join(", ", schema.Properties.Select(p => $"[FromForm] {p.Value.Type.Pascalize()} {p.Key.Camelize()}"));
                }
            }
            return "";
        }

        private static void AddLogic(KeyValuePair<OperationType, OpenApiOperation> operation, string entity, ICodegenOutputFile writer, string? id)
        {
            string parameters = ConcatOperations(operation.Value);
            switch (operation.Key)
            {
                case OperationType.Get when id == null:
                    writer.WriteLine($"var result = await _{entity.Camelize()}Service.Get({parameters});");
                    break;
                case OperationType.Get when id != null:
                    writer.WriteLine($"var result = await _{entity.Camelize()}Service.GetById({parameters});");
                    break;
                case OperationType.Post when operation.Value.Summary.Contains("search"):
                    writer.WriteLine($"var result = await _{entity.Camelize()}Service.Search({parameters});");
                    break;
                case OperationType.Post:
                    writer.WriteLine($"var map = _mapper.Map<Entities.{entity}>(body);");
                    writer.WriteLine($"var result = await _{entity.Camelize()}Service.Post(map);");
                    break;
                case OperationType.Put:
                case OperationType.Patch:
                    writer.WriteLine($"var map = _mapper.Map<Entities.{entity}>(body);");
                    writer.WriteLine($"map?.GetType().GetProperties().FirstOrDefault()?.SetValue(map, {id});");
                    writer.WriteLine($"var result = await _{entity.Camelize()}Service.Put(map);");
                    break;
                case OperationType.Delete:
                    writer.WriteLine($"var result = await _{entity.Camelize()}Service.Delete({id});");
                    break;
                default:
                    writer.WriteLine($"var result = \"method not implemented\";");
                    break;
            }
            writer.WriteLine(TypeReturnStatus(operation.Value.Responses.FirstOrDefault().Key));
        }

        private static string TypeReturnStatus(string status)
        {
            if (status.Equals("204"))
                return $"return StatusCode({status});";
            return $"return StatusCode({status}, result);";
        }

        private static string ConcatOperations(OpenApiOperation operation)
        {
            var parameters = operation.Parameters.Select(p => p.Name.CleanString().Camelize()).ToList();
            if (operation.RequestBody != null)
            {
                if (parameters.Count != 0)
                    parameters.Add("body");
                else
                    parameters.Add("file");
            }
            return string.Join(", ", parameters);
        }

    }
}
