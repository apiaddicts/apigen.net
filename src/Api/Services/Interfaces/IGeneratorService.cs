using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Services
{
    public interface IGeneratorService : IHealthCheck
    {
        byte[] Build(IFormFile? file);
    }
}
