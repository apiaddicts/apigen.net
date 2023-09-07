using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Test
{
    public static class TestUtils
    {
        public static byte[] MockBytes()
        {
            return Encoding.UTF8.GetBytes("This is a dummy file");
        }

        public static IFormFile MockFile(byte[] bytes)
        {
            return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Data", "dummy.txt");
        }

        public static List<string> GetDirectoryContents(string path)
        {
            string[] entries = Directory.GetFileSystemEntries(path);

            var files = new List<string>();
            foreach (string entry in entries)
            {
                if (Directory.Exists(entry))
                {
                    files.AddRange(GetDirectoryContents(entry));
                }
                else
                {
                    files.Add(entry);
                }
            }
            return files;
        }

        public static void AnalysisFiles(List<string> files)
        {
            Assert.NotEmpty(files);

            foreach (var file in files)
            {
                var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));
                var root = tree.GetCompilationUnitRoot();
                Assert.NotNull(root);
            }
        }

    }
}
