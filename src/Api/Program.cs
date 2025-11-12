using Api.Helpers;
using Api.Services;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;

namespace Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder();
            builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
                loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));
            builder.Services.AddOpenApi();
            builder.Services.AddServices();
            builder.Services.AddRouting(options => options.LowercaseUrls = true);
            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    var serverUrl = $"{httpReq.Scheme}://{httpReq.Host.Value}";
                    swaggerDoc.Servers = [
                        new OpenApiServer { Url = "https://api-gateway.apiquality.io/api-apigen-dotnet/v1" },
                        new OpenApiServer { Url = serverUrl }
                        ];
                });
            });
            app.UseSwaggerUI();
            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();
            app.MapControllers();
            app.Run();
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services
                .AddTransient<IGeneratorService, GeneratorService>()
                .AddHealthChecks().AddCheck<IGeneratorService>("generator-service");

            services
                .AddControllers(o => o.Filters.Add(new HttpResponseExceptionFilter()))
                .ConfigureApiBehaviorOptions(o => { o.SuppressModelStateInvalidFilter = true; });

            return services;
        }

        public static IServiceCollection AddOpenApi(this IServiceCollection services)
        {
            services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());
            var openApiInfo = new OpenApiInfo
            {
                Title = "🍩 Apigen ~ dotnet 8",
                Version = "v1",
                License = new OpenApiLicense()
                {
                    Name = "LGPL-3.0 license",
                    Url = new Uri("https://www.gnu.org/licenses/lgpl-3.0.html")
                },
                Contact = new OpenApiContact()
                {
                    Name = "ApiQuality",
                    Url = new Uri("https://apiquality.es"),
                    Email = "contacta@apiquality.es"
                },
                Description =
                """
                Archetype generator in dotnet with hexagonal architecture based on an
                openapi document with extended annotations. This project is a wrapper of the
                java apigen with springboot but using dotnet and adapting some concepts due
                to the paradigm difference. The project is currently being developed by the
                CloudAPPi Services.
                """,
            };
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(openApiInfo.Version, openApiInfo);
                c.ExampleFilters();
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename), true);
            });
            return services;
        }

    }
}