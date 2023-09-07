using Domain.Utils;
using Microsoft.OpenApi.Any;
using Xunit;
using static Domain.Utils.StringUtils;

namespace Test.Domain.Utils
{
    public class StringUtilsTest
    {

        [Fact]
        public void ToCamelCaseTest()
        {
            var result = "This_a_text".ToCamelCase();
            Assert.Equal("thisAText", result);
        }

        [Fact]
        public void ToPascalCaseTest()
        {
            var result = "thisAText".ToPascalCase();
            Assert.Equal("ThisAText", result);
        }

        [Fact]
        public void FormatNameTest()
        {
            var result = FormatName("$this_a_text");
            Assert.Equal("thisAText", result);
        }

        [Fact]
        public void FormatVarTest()
        {
            var result = FormatVar("$This_a_text");
            Assert.Equal("thisAText", result);
        }

        [Fact]
        public void FormatTypeEntityTest()
        {
            var item = new OpenApiString(null);
            var result = FormatTypeEntity("array", item);
            Assert.Equal("List<Entity>?", result);
        }

        [Fact]
        public void CleanStringTest()
        {
            var result = "$-{}".CleanString();
            Assert.Equal("", result);
        }

        [Fact]
        public void GuidIdTest()
        {
            var result = GuidId();
            Assert.NotEmpty(result);
        }

    }
}
