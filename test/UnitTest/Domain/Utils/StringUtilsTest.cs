using Generator.Utils;
using Microsoft.OpenApi.Any;
using static Generator.Utils.StringUtils;

namespace Test.Domain.Utils
{
    public class StringUtilsTest
    {

        [Fact]
        public void FormatTypeEntityTest()
        {
            var item = new OpenApiString(null);
            var result = FormatTypeEntity("array", item);
            Assert.Equal("List<>?", result);
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
