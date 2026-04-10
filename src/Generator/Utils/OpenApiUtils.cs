using Generator.Enums;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Generator.Utils
{
    public static class OpenApiUtils
    {
        
        public static List<(OpenApiTag Tag, OpenApiString? Entity)> TagsByDocPath(OpenApiDocument doc)
        {
            var tags = new List<(OpenApiTag Tag, OpenApiString? Entity)>();
            var seenTagNames = new HashSet<string>();

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
                            if (seenTagNames.Add(tag.Name))
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
            if (doc?.Components?.Extensions is { Count: > 0 } extensions &&
                extensions.TryGetValue("x-apigen-models", out var extensionValue) &&
                extensionValue is OpenApiObject apiObject)
            {
                return apiObject;
            }
            return new OpenApiObject
            {
                ["Sample"] = new OpenApiObject()
            };
        }

        public static string GetProjectName(OpenApiDocument doc)
        {
            var title = doc?.Info?.Title;
            if (string.IsNullOrWhiteSpace(title)) return "Api";
            return string.Concat(
                title.Split([' ', '-', '_'])
                    .Where(s => s.Length > 0)
                    .Select(s => char.ToUpperInvariant(s[0]) + s[1..])
            );
        }

        public static DatabaseType GetDatabaseType(OpenApiDocument doc)
        {
            if (doc?.Extensions is { Count: > 0 } extensions &&
                extensions.TryGetValue("x-apigen-project", out var projectExt) &&
                projectExt is OpenApiObject projectObj &&
                projectObj.TryGetValue("data-driver", out var driverVal) &&
                driverVal is OpenApiString driverStr)
            {
                return driverStr.Value.ToLower() switch
                {
                    "postgresql" => DatabaseType.POSTGRESQL,
                    "mysql" => DatabaseType.MYSQL,
                    _ => DatabaseType.MEMORY
                };
            }
            return DatabaseType.MEMORY;
        }

    }
}
