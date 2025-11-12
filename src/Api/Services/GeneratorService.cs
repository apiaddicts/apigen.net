using Microsoft.Extensions.Diagnostics.HealthChecks;
using static Generator.Build;

namespace Api.Services
{
    public class GeneratorService : IGeneratorService
    {
        public byte[] Build(IFormFile? file)
        {
            return Run(file?.OpenReadStream(), file?.FileName??"template");
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
