using Doc.Examples.Responses;
using Doc.Swagger;
using Domain.Models;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class GeneratorController : ControllerBase
    {

        private readonly IGeneratorService generatorService;

        public GeneratorController(IGeneratorService generatorService)
        {
            this.generatorService = generatorService;
        }


        [HttpPost("file")]
        [SwaggerOperation(Summary = "Create a project from a file")]
        [SwaggerResponse(200, "Generated project compressed in .zip")]
        [SwaggerResponse(400, type: typeof(ErrorResponse))]
        [SwaggerResponse(500, type: typeof(ErrorResponse))]
        [SwaggerResponseExample(500, typeof(ServerErrorExample))]
        [SwaggerResponseExample(400, typeof(BadRequestExample))]
        public IActionResult GenerateFromFile([Required] IFormFile file)
        {
            var output = generatorService.Build(file);
            return File(output, "application/zip", $"{Path.ChangeExtension(file.FileName, "zip")}");
        }

    }
}
