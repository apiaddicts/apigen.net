using Domain.Generators;
using Domain.Utils;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System;
using System.IO;
using Xunit;
using static Test.TestUtils;

namespace Test.Generators
{
    public class GeneratorTest
    {

        private readonly string _pathOpenApi = Path.Combine(AppContext.BaseDirectory, "Resources/api-hospital.yaml");
        private readonly string _tempFilePath = Path.GetTempPath() + @"apigen/test/" + Guid.NewGuid().ToString();
        private OpenApiDocument doc;

        public GeneratorTest()
        {
            FileStream stream = File.OpenRead(_pathOpenApi);
            doc = new OpenApiStreamReader().Read(stream, out var diagnostic);
        }

        [Fact]
        public void StructureGeneratorTest()
        {
            var projectName = "test";
            var projectId = "1234";

            StructureGenerator.Generator(_tempFilePath, projectName, projectId, projectName);
            CloneStaticFiles.Clone(_tempFilePath, projectName, projectId);
            ServiceRegistryGenerator.Generator(doc, _tempFilePath).SaveToFile();
            HttpResponseExceptionFilterGenerator.Generator(doc, _tempFilePath).SaveToFile();
            StandardSearchGenerator.Generator(_tempFilePath).SaveToFile();
            ApigenSelectGenerator.Generator(_tempFilePath).SaveToFile();
            GetDirectoryContents(_tempFilePath);
        }

        [Fact]
        public void ControllersGeneratorTest()
        {
            ControllersGenerator.Generator(doc, _tempFilePath);
            AnalysisFiles(GetDirectoryContents($"{_tempFilePath}/Api/Controllers"));
        }

        [Fact]
        public void HelpersGeneratorTest()
        {
            MappingProfileGenerator.Generator(doc, _tempFilePath).SaveToFile();
            AnalysisFiles(GetDirectoryContents($"{_tempFilePath}/Api/Helpers"));
        }

        [Fact]
        public void UtilsGeneratorTest()
        {
            PageResponseGenerator.Generator(_tempFilePath).SaveToFile();
            UtilsGenerator.GeneratorStringUtils(_tempFilePath).SaveToFile();
            AnalysisFiles(GetDirectoryContents($"{_tempFilePath}/Domain/Utils"));
        }

        [Fact]
        public void ModelsDtoGeneratorTest()
        {
            ModelsDtoGenerator.Generator(doc, _tempFilePath);
            AnalysisFiles(GetDirectoryContents($"{_tempFilePath}/Domain/Models"));
        }

        [Fact]
        public void ModelsEntityGeneratorTest()
        {
            ModelsEntityGenerator.Generator(doc, _tempFilePath);
            AnalysisFiles(GetDirectoryContents($"{_tempFilePath}/Infrastructure/Entities"));
        }

        [Fact]
        public void DbContextGeneratorTest()
        {
            DbContextGenerator.Generator(doc, _tempFilePath).SaveToFile();
            AnalysisFiles(GetDirectoryContents($"{_tempFilePath}/Infrastructure"));
        }

        [Fact]
        public void RepositoryGeneratorTest()
        {
            RepositoryGenerator.Generator(doc, _tempFilePath);
            AnalysisFiles(GetDirectoryContents($"{_tempFilePath}/Infrastructure/Repositories"));
        }

        [Fact]
        public void ServiceGeneratorTest()
        {
            ServiceGenerator.Generator(doc, _tempFilePath);
            AnalysisFiles(GetDirectoryContents($"{_tempFilePath}/Domain/Services"));
        }

        [Fact]
        public void TestGeneratorTest()
        {
            TestGenerator.Generator(doc, _tempFilePath);
            AnalysisFiles(GetDirectoryContents($"{_tempFilePath}/Test"));
        }

    }
}

