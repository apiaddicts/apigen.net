using Microsoft.OpenApi.Any;
using System.Text.RegularExpressions;

namespace Generator.Utils
{
    public static partial class StringUtils
    {

        public static string GuidId()
        {
            return Guid.NewGuid().ToString().ToUpper();
        }

        public static string CleanString(this string text)
        {
            return CleanStringRegex().Replace(text, "");
        }

        public static string FormatType(string? type, string? format = null, string? typeArray = null)
        {
            if (type == null)
                return "";

            switch (type.ToLower())
            {
                case "array":
                    return $"List<{typeArray}>?";
                case "integer":
                case "long":
                case "number":
                    return "long?";
                case "boolean":
                    return "bool?";
                case "localdate":
                case "string" when format != null && format.Equals("date"):
                    return "DateTime?";
                case "string":
                    return "string?";
                default:
                    return $"{type}{format ?? ""}?";
            }
        }

        public static string FormatTypeEntity(string type, OpenApiString itemType)
        {
            if (itemType != null && type.Equals("array", StringComparison.InvariantCultureIgnoreCase))
            {
                return $"List<{itemType.Value}>?";
            }

            return FormatType(type);

        }

        [GeneratedRegex(@"[\/\-{}$]")]
        private static partial Regex CleanStringRegex();
    }
}
