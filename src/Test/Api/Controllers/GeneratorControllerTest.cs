using Api.Controllers;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Test.Api.Controllers
{
    public class GeneratorControllerTest
    {
        private readonly GeneratorController generatorController;
        private readonly Mock<IGeneratorService> generatorService;
        public GeneratorControllerTest()
        {
            generatorService = new Mock<IGeneratorService>();
            generatorController = new GeneratorController(generatorService.Object);
        }

        [Fact]
        public void GeneratingZipByLoadFile()
        {

            var bytes = TestUtils.MockBytes();
            var file = TestUtils.MockFile(bytes);

            generatorService.Setup(x => x.Build(file)).Returns(bytes);

            //Act           
            var result = generatorController.GenerateFromFile(file);
            var response = Assert.IsType<FileContentResult>(result);

            //Assert
            Assert.Equal("application/zip", response.ContentType);
        }


    }
}
