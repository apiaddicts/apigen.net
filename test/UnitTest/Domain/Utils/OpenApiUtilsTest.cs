using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using static Generator.Utils.OpenApiUtils;

namespace Test.Domain.Utils
{
    public class OpenApiUtilsTest
    {

        [Fact]
        public void ListTags()
        {
            var doc = new OpenApiDocument { Paths = new OpenApiPaths() };
            var extensions = new Dictionary<string, IOpenApiExtension> { { "x-apigen-binding", new OpenApiObject { } } };
            var tags = new List<OpenApiTag> { new OpenApiTag { Name = "tag" } };
            var operation = new OpenApiOperation { Tags = tags };
            var operations = new Dictionary<OperationType, OpenApiOperation> { { OperationType.Get, operation } };

            doc.Paths.Add("test", new OpenApiPathItem { Operations = operations, Extensions = extensions });
            var result = TagsByDocPath(doc);
            Assert.Single(result);
        }
    }
}
