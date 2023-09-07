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
    public static class ControllersGenerator
    {
        private static readonly string defaultTag = "default";

        public static void Generator(OpenApiDocument doc, string tempFilePath)
        {
            Log.Debug($"Generate Controllers");
            var tags = TagsByDocPath(doc);

            string ns = "Controllers";
            foreach (var tag in tags)
            {
                GenerateEndpoints(tag, ns, doc, tempFilePath).SaveToFile();
            }

            if (tags.Count.Equals(0))
                GenerateEndpoints((new OpenApiTag() { Name = defaultTag }, null), ns, doc, tempFilePath).SaveToFile();

        }

        public static string? ReadModelInExtensions(KeyValuePair<string, OpenApiPathItem> path, List<string> servicesAlreadyInjected)
        {
            var extensions = (OpenApiObject)path.Value.Extensions.FirstOrDefault(x => x.Key.Equals("x-apigen-binding")).Value;
            if (extensions != null)
            {
                var entity = (OpenApiString)extensions.FirstOrDefault(x => x.Key.Equals("model")).Value;
                if (!servicesAlreadyInjected.Contains(entity.Value))
                {
                    servicesAlreadyInjected.Add(entity.Value);
                    return entity.Value;
                }
            }

            return null;
        }

        private static (ICodegenOutputFile, string?) GenerateEndpoints((OpenApiTag Tag, OpenApiString? Entity) tag, string ns, OpenApiDocument doc, string tempFilePath)
        {
            string cl = $"{FormatName(tag.Tag.Name)}Controller";
            var ctx = new CodegenContext();
            var w = ctx[$"{cl}.cs"];
            var servicesAlreadyInjected = new List<string>();

            w.WriteLine("using Microsoft.AspNetCore.Mvc;");
            w.WriteLine("using Swashbuckle.AspNetCore.Filters;");
            w.WriteLine("using AutoMapper;");
            w.WriteLine("using Services;");
            w.WriteLine("using Models;\n");

            w.WithCurlyBraces($"namespace {ns}", () =>
            {
                w.WriteLine("[ApiController]");
                w.WriteLine("[Route(\"[controller]\")]");
                w.WriteLine("[Produces(\"application/json\")]");
                w.WithCurlyBraces($"public class {cl} : ControllerBase", () =>
                {

                    #region Generate Constructor
                    foreach (var path in doc.Paths)
                    {
                        var findTag = path.Value.Operations.Values.FirstOrDefault(x => x.Tags.Contains(tag.Tag));
                        if (findTag != null)
                        {
                            var entity = ReadModelInExtensions(path, servicesAlreadyInjected);
                            if (entity != null)
                                w.WriteLine($"private readonly {entity}Service _{entity.ToCamelCase()}Service;");
                        }
                    }
                    w.WriteLine("private readonly IMapper _mapper;");
                    w.Write($"\npublic {cl}(IMapper mapper");

                    for (var s = 0; s < servicesAlreadyInjected.Count; s++)
                    {
                        w.Write($", {servicesAlreadyInjected[s]}Service {servicesAlreadyInjected[s].ToCamelCase()}Service");
                    }

                    w.WriteLine($"){{\n\t_mapper = mapper;");

                    foreach (var service in servicesAlreadyInjected)
                        w.WriteLine($"\t_{service.ToCamelCase()}Service = {service.ToCamelCase()}Service;");
                    w.WriteLine($"}}\n");
                    #endregion


                    foreach (var path in doc.Paths)
                    {
                        foreach (var operation in path.Value.Operations)
                        {
                            if (operation.Value.Tags.Contains(tag.Tag) || tag.Tag.Name.Equals(defaultTag))
                            {
                                foreach (var response in operation.Value.Responses)
                                {
                                    foreach (var content in response.Value.Content)
                                    {
                                        if (content.Value.Schema != null && content.Value.Schema.Reference != null)
                                        {
                                            w.WriteLine($"[ProducesResponseType({response.Key}, Type = " +
                                                $"typeof({FormatName(content.Value.Schema.Reference.Id)}Model))]");
                                            w.WriteLine($"[SwaggerResponseExample({response.Key}, " +
                                                $"typeof({FormatName(content.Value.Schema.Reference.Id)}Model))]");
                                        }

                                    }
                                }

                                w.WriteLine($"[Http{FormatName(operation.Key.ToString())}(\"{path.Key}\")]");
                                w.WithCurlyBraces($"public IActionResult " +
                                    $"{operation.Value.OperationId.ToPascalCase()} " +
                                    $"({AddOperations(operation.Value)})", () =>
                                    {
                                        if (servicesAlreadyInjected != null && servicesAlreadyInjected.Any())
                                            AddLogic(operation, servicesAlreadyInjected.First(), w);
                                        else
                                            w.WriteLine($"return StatusCode({operation.Value.Responses.FirstOrDefault().Key}, new NotImplementedException());");
                                    });
                                w.WriteLine();
                            }
                        }
                    }
                });
            });

            return (w, $"{tempFilePath}/Api/{ns}/");
        }

        private static string AddOperations(OpenApiOperation operations)
        {
            StringBuilder builder = new();

            for (int i = 0; i < operations.Parameters.Count; i++)
            {
                var param = operations.Parameters[i];
                if (param.In.Equals(ParameterLocation.Path))
                    builder.Append($"[FromRoute(Name = \"{param.Name}\")] {AddSchema(param.Schema)} {FormatVar(param.Name)}");
                else if (param.In.Equals(ParameterLocation.Query))
                    builder.Append($"[FromQuery(Name = \"{param.Name}\")] {AddSchema(param.Schema)} {FormatVar(param.Name)}");
                else if (param.In.Equals(ParameterLocation.Header))
                    builder.Append($"[FromHeader(Name = \"{param.Name}\")] {AddSchema(param.Schema)} {FormatVar(param.Name)}");

                if (operations.Parameters.Any() && i != operations.Parameters.Count - 1)
                    builder.Append(", ");
            }

            if (operations.RequestBody != null)
            {
                if (builder.Length != 0)
                    builder.Append(", ");

                var content = operations.RequestBody.Content;
                if (content.ContainsKey("multipart/form-data"))
                    builder.Append($"IFormFile file");
                else if (content.ContainsKey("application/json"))
                {
                    var schema = content["application/json"].Schema;
                    if (schema.Reference != null)
                        builder.Append($"[FromBody] {FormatName(schema.Reference.Id)}Model body");
                    else
                    {
                        builder.Append($"[FromBody] dynamic body");
                    }
                }
                else if (content.ContainsKey("application/x-www-form-urlencoded"))
                {
                    var schema = content["application/x-www-form-urlencoded"].Schema;

                    if (schema.Properties != null)
                    {
                        for (int i = 0; i < schema.Properties.Count; i++)
                        {
                            var properties = schema.Properties.ElementAt(i);
                            builder.Append($"[FromForm] {FormatType(properties.Value.Type)} {properties.Key}");

                            if (schema.Properties.Any() && i != schema.Properties.Count - 1)
                                builder.Append(", ");
                        }
                    }

                }

            }

            return builder.ToString();
        }

        private static string ConcatOperations(OpenApiOperation operations)
        {
            StringBuilder builder = new();

            for (int i = 0; i < operations.Parameters.Count; i++)
            {
                var param = operations.Parameters[i];
                builder.Append($"{FormatVar(param.Name)}");

                if (operations.Parameters.Any() && i != operations.Parameters.Count - 1)
                    builder.Append(", ");
            }

            if (operations.RequestBody != null)
            {
                if (builder.Length != 0)
                    builder.Append(", ");

                var content = operations.RequestBody.Content;
                if (content.ContainsKey("multipart/form-data"))
                    builder.Append($"file");
                else if (content.ContainsKey("application/json"))
                {
                    var schema = content["application/json"].Schema;
                    if (schema.Reference != null)
                        builder.Append($"body");
                }
                else if (content.ContainsKey("application/x-www-form-urlencoded"))
                {
                    var schema = content["application/x-www-form-urlencoded"].Schema;

                    if (schema.Properties != null)
                    {
                        for (int i = 0; i < schema.Properties.Count; i++)
                        {
                            var properties = schema.Properties.ElementAt(i);
                            builder.Append($"{properties.Key}");

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
                    return "int";
            }

            if (Schema.Type != null)
            {
                if (Schema.Type.Contains("string"))
                    return "string";
                if (Schema.Type.Contains("array"))
                    return "List<string>";
                if (Schema.Type.Contains("boolean"))
                    return "bool";
            }

            return "object";
        }

        private static void AddLogic(KeyValuePair<OperationType, OpenApiOperation> operation, string entity, ICodegenOutputFile w)
        {
            string concatOperations = ConcatOperations(operation.Value);

            if (operation.Key.Equals(OperationType.Get) && !concatOperations.Contains("id"))
            {
                w.WriteLine($"var result = _{entity.ToCamelCase()}Service.Get({concatOperations});");
            }
            else if (operation.Key.Equals(OperationType.Get) && concatOperations.Contains("id"))
            {
                w.WriteLine($"var result = _{entity.ToCamelCase()}Service.GetById(id);");
            }
            else if (operation.Key.Equals(OperationType.Post) && operation.Value.Summary != null && operation.Value.Summary.Contains("search"))
            {
                w.WriteLine($"var result = _{entity.ToCamelCase()}Service.Get({concatOperations});");
            }
            else if (operation.Key.Equals(OperationType.Post) && concatOperations.Contains("body"))
            {
                w.WriteLine($"var map = _mapper.Map<Entities.{entity}Entity>(body);");
                w.WriteLine($"var result = _{entity.ToCamelCase()}Service.Post(map);");
            }
            else if (operation.Key.Equals(OperationType.Put) && concatOperations.Contains("id, body"))
            {
                w.WriteLine($"var map = _mapper.Map<Entities.{entity}Entity>(body);");
                w.WriteLine($"if (id != null) map.Id = id;");
                w.WriteLine($"var result = _{entity.ToCamelCase()}Service.Put(map);");
            }
            else if (operation.Key.Equals(OperationType.Delete))
            {
                w.WriteLine($"var result = _{entity.ToCamelCase()}Service.Delete(id);");
            }
            else
            {
                w.WriteLine($"var result = \"method not implemented\";");
            }
            w.WriteLine($"return StatusCode({operation.Value.Responses.FirstOrDefault().Key}, result);");

        }

    }
}
