using Api.Helpers;
using Doc.Examples.Responses;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using static Api.Helpers.StaticRegistry;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

var app = Build(builder);

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapHealthChecks("/healthcheck");
app.Run();

[ExcludeFromCodeCoverage]
static WebApplication Build(WebApplicationBuilder builder)
{
    var openApiInfo = new OpenApiInfo
    {
        Title = Title,
        Version = OpenApiVersion,
        Contact = Contact,
        Description = AppDescription,
        License = AppLicense
    };

    builder.Services.AddServices();
    builder.Services
        .AddControllers(o => o.Filters.Add(new HttpResponseExceptionFilter()))
        .ConfigureApiBehaviorOptions(o => { o.SuppressModelStateInvalidFilter = true; });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.EnableAnnotations();
        c.SwaggerDoc(openApiInfo.Version, openApiInfo);
        c.OperationFilter<AddResponseHeadersFilter>();
        c.ExampleFilters();

    });

    builder.Services.AddSwaggerExamplesFromAssemblyOf<ServerErrorExample>();
    builder.Services.AddRouting(options => options.LowercaseUrls = true);
    builder.Services.AddHealthChecks();
    return builder.Build();
}