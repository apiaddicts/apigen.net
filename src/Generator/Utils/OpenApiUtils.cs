using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Generator.Utils
{
    public static class OpenApiUtils
    {
        
        public static List<(OpenApiTag Tag, OpenApiString? Entity)> TagsByDocPath(OpenApiDocument doc)
        {
            var tags = new List<(OpenApiTag Tag, OpenApiString? Entity)>();

            if(doc.Paths != null)
            {
                foreach (var path in doc.Paths)
                {
                    var extensions = GetApiGenModelsOrDefault(doc);
                    OpenApiString? entity = extensions != null ? (OpenApiString)extensions.FirstOrDefault(x => x.Key.Equals("model")).Value : null;

                    foreach (var operation in path.Value.Operations)
                    {
                        foreach (var tag in operation.Value.Tags)
                        {
                            if (!tags.Exists(x => x.Tag.Name.Equals(tag.Name)))
                            {
                                tags.Add((tag, entity));
                            }
                        }
                    }
                }
            }
            
            return tags;
        }

        public static string AddSchema(OpenApiSchema Schema, string optional = "?")
        {
            if (Schema.Format != null)
            {
                switch (Schema.Format.ToLower())
                {
                    case "int32":
                    case "int64":
                        return "int";
                    case "float":
                        return "float";
                    case "double":
                        return "double";
                    case "byte":
                        return "byte";
                    case "binary":
                        return "byte[]";
                    case "date":
                    case "date-time":
                        return "DateTime";
                    default:
                        break;
                }
            }

            if (Schema.Type != null)
            {
                switch (Schema.Type.ToLower())
                {
                    case "boolean":
                        return "bool";
                    case "string":
                        return $"string{optional}";
                    case "object":
                        return "object";
                    case "array":
                        return $"List<{AddSchema(Schema.Items, "")}>?";
                    default:
                        break;
                }
            }

            return "object";
        }

        public static OpenApiObject GetApiGenModelsOrDefault(OpenApiDocument doc)
        {
            if (doc?.Components?.Extensions != null &&
                doc.Components.Extensions.TryGetValue("x-apigen-models", out var extensionValue) &&
                extensionValue is OpenApiObject apiObject)
            {
                return apiObject;
            }
            return new OpenApiObject
            {
                ["Sample"] = new OpenApiObject()
            };
        }

    }
}
