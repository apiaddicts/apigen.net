using Generator.Core;
using Generator.Utils;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using static Test.TestUtils;
using static Generator.Utils.StringUtils;

namespace Test.Generators
{
    public class GeneratorTest : IDisposable
    {

        private readonly string _pathOpenApi = Path.Combine(AppContext.BaseDirectory, "./Examples/api-example.yml");
        private readonly string _tempFilePath = Path.GetTempPath() + @"apigen/test/" + GuidId();
        private OpenApiDocument doc;

        public GeneratorTest()
        {
            FileStream stream = File.OpenRead(_pathOpenApi);
            doc = new OpenApiStreamReader().Read(stream, out var diagnostic);
        }

        public void Dispose()
        {
            Directory.Delete(_tempFilePath, true);
            GC.Collect();
        }

        [Fact]
        public void StructureGeneratorTest()
        {
            var projectName = "test";
            var projectId = "1234";

            StructureGenerator.Definelayers(projectName);
            StructureGenerator.Generator(_tempFilePath, projectName, projectId, projectName);
            CreateStaticFiles.Generate(_tempFilePath, projectName, projectId);
            ServiceRegistryGenerator.Generator(doc, _tempFilePath).SaveToFile();
            HttpResponseExceptionFilterGenerator.Generator(_tempFilePath).SaveToFile();
            GetDirectoryContents(_tempFilePath);
        }

        [Fact]
        public void ControllersGeneratorTest()
        {
            ControllersGenerator.Generator(doc, _tempFilePath);
            AnalysisFiles(GetDirectoryContents($"{_tempFilePath}/src/Api/Controllers"));
        }

        [Fact]
        public void HelpersGeneratorTest()
        {
            MappingProfileGenerator.Generator(doc, _tempFilePath).SaveToFile();
            AnalysisFiles(GetDirectoryContents($"{_tempFilePath}/src/Api/Helpers"));
        }

        [Fact]
        public void ModelsDtoGeneratorTest()
        {
            ModelsDtoGenerator.Generator(doc, _tempFilePath);
            AnalysisFiles(GetDirectoryContents($"{_tempFilePath}/src/Domain/Models"));
        }

        [Fact]
        public void ModelsEntityGeneratorTest()
        {
            ModelsEntityGenerator.Generator(doc, _tempFilePath);
            AnalysisFiles(GetDirectoryContents($"{_tempFilePath}/src/Infrastructure/Entities"));
        }

        [Fact]
        public void DbContextGeneratorTest()
        {
            DbContextGenerator.Generator(doc, _tempFilePath).SaveToFile();
            AnalysisFiles(GetDirectoryContents($"{_tempFilePath}/src/Infrastructure"));
        }

        [Fact]
        public void RepositoryGeneratorTest()
        {
            RepositoryGenerator.Generator(doc, _tempFilePath);
            AnalysisFiles(GetDirectoryContents($"{_tempFilePath}/src/Infrastructure/Repositories"));
        }

        [Fact]
        public void ServiceGeneratorTest()
        {
            ServiceGenerator.Generator(doc, _tempFilePath);
            AnalysisFiles(GetDirectoryContents($"{_tempFilePath}/src/Domain/Services"));
        }

        [Fact]
        public void TestGeneratorTest()
        {
            TestGenerator.Generator(doc, _tempFilePath);
            AnalysisFiles(GetDirectoryContents($"{_tempFilePath}/test/UnitTest"));
        }

        [Fact]
        public void UtilsGeneratorTest()
        {
            ApigenOperationsGenerator.Generator(_tempFilePath).SaveToFile();
            StringUtilsGenerator.Generator(_tempFilePath).SaveToFile();
            CustomExceptionGenerator.Generator(_tempFilePath).SaveToFile();
            PageResponseGenerator.Generator(_tempFilePath).SaveToFile();
            AnalysisFiles(GetDirectoryContents($"{_tempFilePath}/src/Domain/Utils"));
        }

    }
}

