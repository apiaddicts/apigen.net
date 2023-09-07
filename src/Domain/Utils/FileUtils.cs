using CodegenCS;
using CodegenCS.IO;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Domain.Utils
{
    public static class FileUtils
    {
        public static Stream ReadStream(IFormFile file)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return new MemoryStream(GetBytes(file));
        }

        public static byte[] GetBytes(IFormFile formFile)
        {
            using var memoryStream = new MemoryStream();
            formFile.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
        [ExcludeFromCodeCoverage]
        public static string ReadResource(string path)
        {
            return File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + path);
        }
        [ExcludeFromCodeCoverage]
        public static void SaveFileStream(string path, Stream stream)
        {
            var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fileStream);
            stream.Dispose();
            fileStream.Dispose();
        }
        [ExcludeFromCodeCoverage]
        public static (ICodegenOutputFile, string?) SaveToFile(this (ICodegenOutputFile, string?) t, bool save = true)
        {
            if (!string.IsNullOrEmpty(t.Item2) && save)
                t.Item1.SaveToFolder(t.Item2);
            return t;
        }

    }
}
