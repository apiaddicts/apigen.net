using Microsoft.OpenApi.Any;
using Newtonsoft.Json.Serialization;
using System.Text.RegularExpressions;

namespace Domain.Utils
{
    public static class StringUtils
    {

        public static string FormatName(string text)
        {
            return text
                .ToCamelCase()
                .ToPascalCase()
                .CleanString();
        }

        public static string FormatVar(string text)
        {
            return text
                .ToCamelCase()
                .CleanString();
        }

        public static string CleanString(this string text)
        {
            return text
            .Replace("/", "")
            .Replace("-", "")
            .Replace("{", "")
            .Replace("}", "")
            .Replace("$", "");
        }

        public static string ToCamelCase(this string s1)
        {
            s1 = s1.ToLower();
            return Regex.Replace(s1, "_[a-z]", delegate (Match m)
            {
                return m.ToString().TrimStart('_').ToUpper();
            });
        }

        public static string FormatType(string? type, string? format = null)
        {

            if (type == null)
                return "";

            if (type.Equals("array", StringComparison.InvariantCultureIgnoreCase))
            {
                return "List<string>?";
            }
            else if (type.Equals("integer", StringComparison.InvariantCultureIgnoreCase))
            {
                return "int?";
            }
            else if (type.Equals("boolean", StringComparison.InvariantCultureIgnoreCase))
            {
                return "bool?";
            }
            else if (type.Equals("long", StringComparison.InvariantCultureIgnoreCase))
            {
                return "long?";
            }
            else if (type.Equals("number", StringComparison.InvariantCultureIgnoreCase))
            {
                return "long?";
            }
            else if (type.Equals("LocalDate", StringComparison.InvariantCultureIgnoreCase))
            {
                return "DateTime?";
            }
            else if (type.Equals("String", StringComparison.InvariantCultureIgnoreCase))
            {
                if (format != null && format.Equals("date"))
                    return "DateTime?";

                return "string?";
            }

            var more = format != null ? format : "";
            return $"{type}{more}?";
        }

        public static string FormatTypeEntity(string type, OpenApiString itemType)
        {
            if (itemType != null)
            {
                if (type.Equals("array", StringComparison.InvariantCultureIgnoreCase))
                {
                    return $"List<{itemType.Value}Entity>?";
                }
            }

            return FormatType(type, "Entity");

        }

        public static string GuidId()
        {
            return Guid.NewGuid().ToString().ToUpper();
        }

        public static string ToPascalCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return char.ToUpperInvariant(str[0]) + str[1..];
            }
            return str;
        }

        public static string? ToSnakeCase(this string? str) => str is null
        ? null
        : new DefaultContractResolver() { NamingStrategy = new SnakeCaseNamingStrategy() }.GetResolvedPropertyName(str);

    }
}
