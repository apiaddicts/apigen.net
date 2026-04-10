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
            string projectName = OpenApiUtils.GetProjectName(doc);
            string ns = $"{projectName}.Api.Controllers";

            foreach (var tag in tags)
            {
                GenerateControllerForTag(tag, ns, doc, tempFilePath, projectName).SaveToFile();
            }

            if (tags.Count == 0)
            {
                GenerateControllerForTag((new OpenApiTag() { Name = defaultTag }, null), ns, doc, tempFilePath, projectName).SaveToFile();
            }
        }

        private static (ICodegenOutputFile, string?) GenerateControllerForTag((OpenApiTag Tag, OpenApiString? Entity) tag, string ns, OpenApiDocument doc, string tempFilePath, string projectName)
        {
            string className = $"{tag.Tag.Name.Pascalize()}Controller";
            var context = new CodegenContext();
            var writer = context[$"{className}.cs"];
            var servicesAlreadyInjected = new List<string>();

            var (hasServices, hasModels, hasEntities) = ScanRequiredUsings(doc, tag, servicesAlreadyInjected);
            DefineControllerUsingStatements(writer, projectName, hasServices, hasModels, hasEntities);
            DefineControllerNamespace(writer, ns, className, doc, tag, servicesAlreadyInjected, projectName);

            return (writer, $"{tempFilePath}/src/Api/{ns.Split('.').Last()}/");
        }

        private static (bool hasServices, bool hasModels, bool hasEntities) ScanRequiredUsings(OpenApiDocument doc, (OpenApiTag Tag, OpenApiString? Entity) tag, List<string> servicesAlreadyInjected)
        {
            bool hasModels = false;
            bool hasEntities = false;

            if (doc.Paths == null) return (false, false, false);

            foreach (var path in doc.Paths)
            {
                if (path.Value.Operations.Values.Any(x => x.Tags.Contains(tag.Tag)))
                    ReadModelInExtensions(path, servicesAlreadyInjected);

                foreach (var operation in path.Value.Operations)
                {
                    if (!operation.Value.Tags.Contains(tag.Tag) && !tag.Tag.Name.Equals(defaultTag)) continue;

                    if (operation.Value.RequestBody?.Content.TryGetValue("application/json", out var appJson) == true)
                    {
                        if (appJson.Schema?.Reference != null) hasModels = true;
                        if (operation.Key is OperationType.Post or OperationType.Put or OperationType.Patch) hasEntities = true;
                    }
                }
            }

            return (servicesAlreadyInjected.Count > 0, hasModels, hasEntities);
        }

        private static void DefineControllerUsingStatements(ICodegenOutputFile writer, string projectName, bool hasServices, bool hasModels, bool hasEntities)
        {
            writer.WriteLine("using Microsoft.AspNetCore.Mvc;");
            writer.WriteLine("using AutoMapper;");
            if (hasServices) writer.WriteLine($"using {projectName}.Domain.Services;");
            if (hasModels)   writer.WriteLine($"using Models = {projectName}.Domain.Models;");
            if (hasEntities) writer.WriteLine($"using Entities = {projectName}.Infrastructure.Entities;");
            writer.WriteLine();
        }

        private static void DefineControllerNamespace(ICodegenOutputFile writer, string ns, string className, OpenApiDocument doc, (OpenApiTag Tag, OpenApiString? Entity) tag, List<string> servicesAlreadyInjected, string projectName)
        {
            writer.WithCurlyBraces($"namespace {ns}", () =>
            {
                DefineControllerClass(writer, className, doc, tag, servicesAlreadyInjected, projectName);
            });
        }

        private static void DefineControllerClass(ICodegenOutputFile writer, string className, OpenApiDocument doc, (OpenApiTag Tag, OpenApiString? Entity) tag, List<string> servicesAlreadyInjected, string projectName)
        {
            writer.WriteLine("[ApiController]");
            writer.WriteLine("[Route(\"[controller]\")]");
            writer.WriteLine("[Produces(\"application/json\")]");

            writer.WithCurlyBraces($"public class {className} : ControllerBase", () =>
            {
                AddControllerConstructor(writer, className, doc, tag, servicesAlreadyInjected, projectName);
                AddControllerEndpoints(writer, doc, tag, servicesAlreadyInjected, projectName);
            });
        }

        private static void AddControllerConstructor(ICodegenOutputFile writer, string className, OpenApiDocument doc, (OpenApiTag Tag, OpenApiString? Entity) tag, List<string> servicesAlreadyInjected, string projectName)
        {
            if (doc.Paths!=null)
            {
                foreach (var service in servicesAlreadyInjected)
                {
                    writer.WriteLine($"private readonly {service}Service _{service.Camelize()}Service;");
                }

                writer.WriteLine("private readonly IMapper _mapper;");
                writer.Write($"public {className}(IMapper mapper");
                foreach (var service in servicesAlreadyInjected)
                {
                    writer.Write($", {service}Service {service.Camelize()}Service");
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

        private static void AddControllerEndpoints(ICodegenOutputFile writer, OpenApiDocument doc, (OpenApiTag Tag, OpenApiString? Entity) tag, List<string> servicesAlreadyInjected, string projectName)
        {
            if (doc.Paths != null)
            {
                foreach (var path in doc.Paths)
                {
                    foreach (var operation in path.Value.Operations)
                    {
                        if (operation.Value.Tags.Contains(tag.Tag) || tag.Tag.Name.Equals(defaultTag))
                        {
                            DefineEndpointMethod(writer, path.Key, operation, servicesAlreadyInjected, projectName);
                        }
                    }
                }
            }

        }

        private static void DefineEndpointMethod(ICodegenOutputFile writer, string pathKey, KeyValuePair<OperationType, OpenApiOperation> operation, List<string> servicesAlreadyInjected, string projectName)
        {
            writer.WriteLine($"[Http{operation.Key.ToString().Pascalize()}(\"{pathKey}\")]");

            // Add [Consumes] attribute for file uploads
            if (operation.Value.RequestBody != null)
            {
                var content = operation.Value.RequestBody.Content;
                if (content.ContainsKey("multipart/form-data"))
                {
                    writer.WriteLine($"[Consumes(\"multipart/form-data\")]");
                }
                else if (content.ContainsKey("application/octet-stream"))
                {
                    writer.WriteLine($"[Consumes(\"application/octet-stream\")]");
                }
            }

            writer.WithCurlyBraces($"public async Task<IActionResult> {operation.Value.OperationId.Pascalize()}({AddOperations(operation.Value, projectName)})", () =>
            {
                if (servicesAlreadyInjected.Count != 0)
                {
                    var id = operation.Value.Parameters?.FirstOrDefault(x => x.In == ParameterLocation.Path)?.Name.Camelize();
                    AddLogic(operation, servicesAlreadyInjected[0], writer, id, projectName);
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

        private static string AddOperations(OpenApiOperation operation, string projectName)
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
                return "[FromForm] IFormFile file";
            }
            else if (content.ContainsKey("application/octet-stream"))
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

        private static void AddLogic(KeyValuePair<OperationType, OpenApiOperation> operation, string entity, ICodegenOutputFile writer, string? id, string projectName)
        {
            bool isFileUpload = operation.Value.RequestBody?.Content.ContainsKey("multipart/form-data") == true ||
                                operation.Value.RequestBody?.Content.ContainsKey("application/octet-stream") == true;
            bool hasRequestBody = operation.Value.RequestBody != null && 
                                  operation.Value.RequestBody.Content.ContainsKey("application/json");

            switch (operation.Key)
            {
                case OperationType.Get when id == null:
                    writer.WriteLine($"var result = await _{entity.Camelize()}Service.Get({BuildGetParameters(operation.Value)});");
                    break;
                case OperationType.Get when id != null:
                    writer.WriteLine($"var result = await _{entity.Camelize()}Service.GetById({BuildGetByIdParameters(operation.Value, id)});");
                    break;
                case OperationType.Post when operation.Value.Summary?.Contains("search") == true:
                    writer.WriteLine($"var result = await _{entity.Camelize()}Service.Search({ConcatOperations(operation.Value)});");
                    break;
                case OperationType.Post when isFileUpload:
                    writer.WriteLine($"// File upload logic - implement according to your requirements");
                    writer.WriteLine($"var result = new {{ FileName = file.FileName, Size = file.Length }};");
                    break;
                case OperationType.Post when hasRequestBody:
                    writer.WriteLine($"var map = _mapper.Map<Entities.{entity}>(body);");
                    writer.WriteLine($"var result = await _{entity.Camelize()}Service.Post(map);");
                    break;
                case OperationType.Post:
                    writer.WriteLine($"// POST without request body - implement custom logic as needed");
                    writer.WriteLine($"var result = new {{ Message = \"Operation completed\" }};");
                    break;
                case OperationType.Put when hasRequestBody:
                case OperationType.Patch when hasRequestBody:
                    writer.WriteLine($"var map = _mapper.Map<Entities.{entity}>(body);");
                    if (!string.IsNullOrEmpty(id))
                        writer.WriteLine($"map?.GetType().GetProperties().FirstOrDefault()?.SetValue(map, {id});");
                    writer.WriteLine($"var result = await _{entity.Camelize()}Service.Put(map!);");
                    break;
                case OperationType.Put:
                case OperationType.Patch:
                    writer.WriteLine($"// PUT/PATCH without request body - implement custom logic as needed");
                    writer.WriteLine($"var result = new {{ Message = \"Operation completed\" }};");
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
                var content = operation.RequestBody.Content;
                if (content.ContainsKey("multipart/form-data") || content.ContainsKey("application/octet-stream"))
                {
                    parameters.Add("file");
                }
                else if (content.ContainsKey("application/json"))
                {
                    parameters.Add("body");
                }
            }
            return string.Join(", ", parameters);
        }

        private static string BuildGetParameters(OpenApiOperation operation)
        {
            var paramMap = operation.Parameters.ToDictionary(p => p.Name.CleanString().ToLower(), p => p.Name.CleanString().Camelize());
            
            var init = paramMap.ContainsKey("init") ? paramMap["init"] : "1";
            var limit = paramMap.ContainsKey("limit") ? paramMap["limit"] : "10";
            var total = paramMap.ContainsKey("total") ? paramMap["total"] : "false";
            var orderby = paramMap.ContainsKey("orderby") ? paramMap["orderby"] : "null";
            var select = paramMap.ContainsKey("select") ? paramMap["select"] : "null";
            var exclude = paramMap.ContainsKey("exclude") ? paramMap["exclude"] : "null";
            var expand = paramMap.ContainsKey("expand") ? paramMap["expand"] : "null";
            var filter = paramMap.ContainsKey("filter") ? paramMap["filter"] : "null";

            return $"{init}, {limit}, {total}, {orderby}, {select}, {exclude}, {expand}, {filter}";
        }

        private static string BuildGetByIdParameters(OpenApiOperation operation, string id)
        {
            var paramMap = operation.Parameters.ToDictionary(p => p.Name.CleanString().ToLower(), p => p.Name.CleanString().Camelize());
            
            var select = paramMap.ContainsKey("select") ? paramMap["select"] : "null";
            var exclude = paramMap.ContainsKey("exclude") ? paramMap["exclude"] : "null";
            var expand = paramMap.ContainsKey("expand") ? paramMap["expand"] : "null";

            return $"{id}, {select}, {exclude}, {expand}";
        }

    }
}
