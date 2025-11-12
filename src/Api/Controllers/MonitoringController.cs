using Api.Models.Out;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Swashbuckle.AspNetCore.Filters;

namespace Api.Controllers
{

    [ApiController]
    [Produces("application/json")]
    public class MonitoringController(HealthCheckService healthCheckService) : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService = healthCheckService;

        [HttpGet("status")]
        [ProducesResponseType(statusCode: 200, type: typeof(StatusResponse))]
        [SwaggerResponseExample(statusCode: 200, examplesProviderType: typeof(StatusResponseExample))]
        public async Task<IActionResult> Ping()
        {
            var report = await _healthCheckService.CheckHealthAsync();
            var response = report.Entries.Select(entry => new StatusResponse
            {
                SystemName = entry.Key,
                Status = entry.Value.Status == HealthStatus.Healthy
            }).ToList();
            return Ok(response);
        }
    }
}
