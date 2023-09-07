using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Domain.Utils
{
    public static class OpenApiUtils
    {

        public static List<(OpenApiTag Tag, OpenApiString? Entity)> TagsByDocPath(OpenApiDocument doc)
        {
            var tags = new List<(OpenApiTag Tag, OpenApiString? Entity)>();

            foreach (var path in doc.Paths)
            {
                OpenApiString? entity = null;
                var extensions = (OpenApiObject)path.Value.Extensions.FirstOrDefault(x => x.Key.Equals("x-apigen-binding")).Value;

                if (extensions != null)
                    entity = (OpenApiString)extensions.FirstOrDefault(x => x.Key.Equals("model")).Value;

                foreach (var operation in path.Value.Operations)
                {
                    foreach (var tag in operation.Value.Tags)
                    {
                        if (tags.All(x => !x.Item1.Name.Equals(tag.Name)))
                        {
                            tags.Add(new(tag, entity));
                        }
                    }
                }
            }

            return tags;
        }
    }
}
