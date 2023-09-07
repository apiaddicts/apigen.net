using Domain.Enums;
using Domain.Generators;
using Ionic.Zip;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using static Domain.Utils.FileUtils;
using static Domain.Utils.StringUtils;

namespace Domain.Services
{
    [ExcludeFromCodeCoverage]
    public class GeneratorService : IGeneratorService
    {
        public byte[] Build(IFormFile file)
        {
            var tempFilePath = Path.GetTempPath() + @"apigen/" + Guid.NewGuid().ToString();
            Log.Debug($"Temporal Path: {tempFilePath}");
            if (!Directory.Exists(tempFilePath)) Directory.CreateDirectory(tempFilePath);

            OpenApiDocument doc = new OpenApiStreamReader().Read(ReadStream(file), out var diagnostic);

            DatabaseType databaseType = DatabaseType.MEMORY;
            var projectInfo = (OpenApiObject)doc.Extensions.FirstOrDefault(x => x.Key.Equals("x-apigen-project")).Value;

            if (projectInfo != null)
            {
                var relationalbd = (OpenApiObject)projectInfo.FirstOrDefault(x => x.Key.Equals("relational-persistence")).Value;

                if (relationalbd != null)
                {
                    Enum.TryParse<DatabaseType>(((OpenApiString)relationalbd.FirstOrDefault(x => x.Key.Equals("type")).Value).Value, true, out databaseType);
                }
            }

            if (diagnostic.Errors.Any())
                Log.Error($"OpenApi: {diagnostic.Errors}");

            var projectId = GuidId();
            var projectName = doc.Info.Title.Replace(" ", "");
            StructureGenerator.Generator(tempFilePath, projectName, projectId, file.FileName, databaseType);
            CloneStaticFiles.Clone(tempFilePath, projectName, projectId);
            MappingProfileGenerator.Generator(doc, tempFilePath).SaveToFile();
            PageResponseGenerator.Generator(tempFilePath).SaveToFile();
            UtilsGenerator.GeneratorStringUtils(tempFilePath).SaveToFile();
            ControllersGenerator.Generator(doc, tempFilePath);
            ModelsDtoGenerator.Generator(doc, tempFilePath);
            ModelsEntityGenerator.Generator(doc, tempFilePath);
            DbContextGenerator.Generator(doc, tempFilePath).SaveToFile();
            RepositoryGenerator.Generator(doc, tempFilePath);
            ServiceGenerator.Generator(doc, tempFilePath);
            ServiceRegistryGenerator.Generator(doc, tempFilePath).SaveToFile();
            HttpResponseExceptionFilterGenerator.Generator(doc, tempFilePath).SaveToFile();
            StandardSearchGenerator.Generator(tempFilePath).SaveToFile();
            ApigenSelectGenerator.Generator(tempFilePath).SaveToFile();
            TestGenerator.Generator(doc, tempFilePath);

            var pathOpenApi = $"{tempFilePath}/Api/wwwroot/swagger/";
            if (!Directory.Exists(pathOpenApi)) Directory.CreateDirectory(pathOpenApi);
            SaveFileStream($"{pathOpenApi}{file.FileName}", ReadStream(file));

            using ZipFile zip = new();
            zip.AddDirectory(tempFilePath);
            MemoryStream output = new();
            zip.Save(output);
            Directory.Delete(tempFilePath, true);
            return output.ToArray();
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy("A healthy result."));
        }
    }
}
