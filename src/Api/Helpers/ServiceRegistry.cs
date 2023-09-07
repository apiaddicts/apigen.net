using Domain.Services;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection
{

    [ExcludeFromCodeCoverage]
    public static class ServiceRegistry
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services
                .AddTransient<IGeneratorService, GeneratorService>()
                .AddHealthChecks().AddCheck<IGeneratorService>("generator service is healthy");
            return services;
        }
    }
}
