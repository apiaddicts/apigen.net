using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Domain.Services
{
    public interface IGeneratorService : IHealthCheck
    {
        byte[] Build(IFormFile file);
    }
}
