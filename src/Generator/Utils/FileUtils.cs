using CodegenCS;
using CodegenCS.IO;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Serilog;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace Generator.Utils
{
    public static class FileUtils
    {
        public static (ICodegenOutputFile, string?) SaveToFile(this (ICodegenOutputFile, string?) t, bool save = true)
        {
            if (!string.IsNullOrEmpty(t.Item2) && save)
                t.Item1.SaveToFolder(t.Item2);
            return t;
        }

        public static Stream ReadStream(Stream file)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return new MemoryStream(GetBytes(file));
        }

        public static byte[] GetBytes(Stream formFile)
        {
            using var memoryStream = new MemoryStream();
            formFile.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        public static byte[] CompressDirectory(string directoryPath)
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var directoryInfo = new DirectoryInfo(directoryPath);
                foreach (var file in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(directoryPath, file.FullName);
                    var entry = archive.CreateEntry(relativePath, CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    using var fileStream = file.OpenRead();
                    fileStream.CopyTo(entryStream);
                }
            }
            return memoryStream.ToArray();
        }

        public static void SaveFileStream(string path, Stream stream)
        {
            var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fileStream);
            stream.Dispose();
            fileStream.Dispose();
        }

        public static string ReadResource(string path)
        {
            return File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + path);
        }

        public static void CopyOpenApi(string tempFilePath, string fileName, Stream? file)
        {
            if (file == null) return;
            Log.Debug("Copying ~ '{FileName}' in wwwroot/swagger", fileName);
            var pathOpenApi = Path.Combine(tempFilePath, "src", "Api", "wwwroot", "swagger");
            Directory.CreateDirectory(pathOpenApi);
            file.Seek(0, SeekOrigin.Begin);
            SaveFileStream(Path.Combine(pathOpenApi, fileName), ReadStream(file));
        }

        public static OpenApiDocument ReadOpenApi(Stream? file, string filename)
        {
            if (file == null)
            {
                return new OpenApiDocument()
                {
                    Tags = [new() { Name = "Example"}],
                    Info = new()
                    {
                        Title = filename
                    },
                    Paths = new OpenApiPaths
                    {
                        ["/example"] = new OpenApiPathItem
                        {
                            Operations = new Dictionary<OperationType, OpenApiOperation>
                            {
                                
                                [OperationType.Get] = new OpenApiOperation
                                {
                                    OperationId = "GetExample",
                                    Tags = [new() { Name = "Example" }],
                                    Responses = new OpenApiResponses
                                    {
                                        ["200"] = new OpenApiResponse
                                        {
                                            Description = "OK"
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
            }

            var reader = new OpenApiStreamReader();
            var document = reader.Read(ReadStream(file), out var diagnostic);
            Log.Debug("Diagnostic OpenApi ~ {Diagnostic}", JsonSerializer.Serialize(diagnostic));
            return document;
        }

        public static byte[] CompressBuildAndDeleteTempFilePath(string tempFilePath)
        {
            Log.Debug("Compress Directory ~ {TempFilePath}", tempFilePath);
            byte[] compressedData = CompressDirectory(tempFilePath);
            Log.Debug("Delete Directory ~ {TempFilePath}", tempFilePath);
            Directory.Delete(tempFilePath, true);
            return compressedData;
        }

    }
}
