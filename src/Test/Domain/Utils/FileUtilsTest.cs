using Xunit;
using static Domain.Utils.FileUtils;

namespace Test.Domain.Utils
{
    public class FileUtilsTest
    {
        [Fact]
        public void ReadStreamNotNull()
        {
            var bytes = TestUtils.MockBytes();
            var file = TestUtils.MockFile(bytes);
            var result = ReadStream(file);
            Assert.NotNull(result);
        }

        [Fact]
        public void GetBytesNotNull()
        {
            var bytes = TestUtils.MockBytes();
            var file = TestUtils.MockFile(bytes);
            var result = GetBytes(file);
            Assert.NotNull(result);
        }

    }
}
