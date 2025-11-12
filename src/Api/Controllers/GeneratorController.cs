using Api.Models.Out;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Api.Controllers
{
    /// <summary>
    /// Generates an archetype in asp.net from an openapi json/yml document and compresses it in .zip
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class GeneratorController(IGeneratorService generatorService) : ControllerBase
    {

        private readonly IGeneratorService generatorService = generatorService;

        [Produces("application/zip")]
        [SwaggerResponse(statusCode: 200, type: typeof(byte[]))]
        [SwaggerResponse(statusCode: 400, type: typeof(ErrorsResponse))]
        [SwaggerResponseExample(statusCode: 400, examplesProviderType: typeof(BadRequestExample))]
        [SwaggerResponse(statusCode: 500, type: typeof(ErrorsResponse))]
        [SwaggerResponseExample(statusCode: 500, examplesProviderType: typeof(DefaultErrorExample))]
        [HttpPost("file")]
        public IActionResult GenerateFromFile(IFormFile? file)
        {
            var output = generatorService.Build(file);
            return File(output, "application/zip", $"{Path.ChangeExtension(file?.FileName ?? "template", "zip")}");
        }

    }
}
