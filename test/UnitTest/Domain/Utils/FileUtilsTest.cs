using static Generator.Utils.FileUtils;

namespace Test.Domain.Utils
{
    public class FileUtilsTest
    {
        [Fact]
        public void ReadStreamNotNull()
        {
            var bytes = TestUtils.MockBytes();
            var file = TestUtils.MockFile(bytes);
            var result = ReadStream(file.OpenReadStream());
            Assert.NotNull(result);
        }

        [Fact]
        public void GetBytesNotNull()
        {
            var bytes = TestUtils.MockBytes();
            var file = TestUtils.MockFile(bytes);
            var result = GetBytes(file.OpenReadStream());
            Assert.NotNull(result);
        }

    }
}
