using CodegenCS;
using Generator.Utils;
using Humanizer;
using Serilog;
using static Generator.Utils.FileUtils;
using static Generator.Utils.StringUtils;

namespace Generator.Core
{
    record PackageReference(string Name, string Version);

    public static class StructureGenerator
    {
        public static readonly string TargetFramework = "net8.0";
        private static readonly PackageReference VisualStudioVersion = new("VisualStudioVersion", "17.5.33414.496");
        private static readonly PackageReference MinimumVisualStudioVersion = new("MinimumVisualStudioVersion", "10.0.40219.1");
        private static readonly PackageReference VisualStudioFormatVersion = new("Microsoft Visual Studio Solution File, Format Version", "12.00");
        private static readonly Lazy<Task<Dictionary<string, string>>> _nugetVersionsLazy =
            new(() => NuGetVersionFetcher.GetAllPackageReferencesAsync(TargetFramework));

        private static Dictionary<string, string> Layers = [];
        private static Dictionary<string, string> NugetVersions => _nugetVersionsLazy.Value.Result;
        

        public static void Definelayers(string layerName)
        {
            Layers = new Dictionary<string, string>
            {
                {"Api", $"{layerName}.Api"},
                {"Domain", $"{layerName}.Domain"},
                {"Infrastructure", $"{layerName}.Infrastructure"},

                {"Api.Test", $"{layerName}.Api.Tests"},
                {"Domain.Test", $"{layerName}.Domain.Tests"},
                {"Infrastructure.Test", $"{layerName}.Infrastructure.Tests"},
            };
        }



        public static void Generator(string tempFilePath, string projectName, string projectId, string fileName)
        {
            Log.Debug("Generating ~ Structure: {ProjectName}: {ProjectId}", projectName, projectId);
            GenerateSln(tempFilePath, projectName, projectId).SaveToFile();
            GenerateCsProj(tempFilePath, fileName);
            InitProgram(tempFilePath, projectName, fileName).SaveToFile();
        }

        public static (ICodegenOutputFile, string?) GenerateSln(string tempFilePath, string projectName, string projectGuid)
        {
            Log.Debug("Generating ~ Sln: {ProjectGuid}", projectGuid);
            var ctx = new CodegenContext();
            var w = ctx[$"{projectName.Dehumanize()}.sln"];

            //Layers
            var guidApi = GuidId();
            var guidDomain = GuidId();
            var guidInfrastructure = GuidId();

            w.WriteLine($$"""
            {{VisualStudioFormatVersion.Name}} {{VisualStudioFormatVersion.Version}}
            {{VisualStudioVersion.Name}} = {{VisualStudioVersion.Version}}
            {{MinimumVisualStudioVersion.Name}} = {{MinimumVisualStudioVersion.Version}}
            Project("{{{projectGuid}}}") = "Api", "src\Api\{{Layers["Api"]}}.csproj", "{{{guidApi}}}"
            EndProject
            Project("{{{projectGuid}}}") = "Domain", "src\Domain\{{Layers["Domain"]}}.csproj", "{{{guidDomain}}}"
            EndProject
            Project("{{{projectGuid}}}") = "Infrastructure", "src\Infrastructure\{{Layers["Infrastructure"]}}.csproj", "{{{guidInfrastructure}}}"
            EndProject
            """);

            //Tests
            var guidApiTest = GuidId();
            var guidDomainTest = GuidId();
            var guidInfrastructureTest = GuidId();

            w.WriteLine($$"""
            Project("{{{projectGuid}}}") = "Api.Test", "test\UnitTest\Api\{{Layers["Api.Test"]}}.csproj", "{{{guidApiTest}}}"
            EndProject
            Project("{{{projectGuid}}}") = "Domain.Test", "test\UnitTest\Domain\{{Layers["Domain.Test"]}}.csproj", "{{{guidDomainTest}}}"
            EndProject
            Project("{{{projectGuid}}}") = "Infrastructure.Test", "test\UnitTest\Infrastructure\{{Layers["Infrastructure.Test"]}}.csproj", "{{{guidInfrastructureTest}}}"
            EndProject
            """);

            //Folders
            var guildFolderProject = "2150E333-8FDC-42A3-9474-1A3956D46DE8";
            var guidFolderApi = GuidId();
            var guidFolderDomain = GuidId();
            var guidFolderInfrastructure = GuidId();

            w.WriteLine($$"""
            Project("{{{guildFolderProject}}}") = "1.Api", "1.Api", "{{{guidFolderApi}}}"
            EndProject
            Project("{{{guildFolderProject}}}") = "2.Domain", "2.Domain", "{{{guidFolderDomain}}}"
            EndProject
            Project("{{{guildFolderProject}}}") = "3.Infrastructure", "3.Infrastructure", "{{{guidFolderInfrastructure}}}"
            EndProject
            """);

            //Relation Layers - Folders
            w.WriteLine($$"""
            Global
                GlobalSection(NestedProjects) = preSolution
            	    {{{guidApi}}} = {{{guidFolderApi}}}
            	    {{{guidDomain}}} = {{{guidFolderDomain}}}
            	    {{{guidInfrastructure}}} = {{{guidFolderInfrastructure}}}
            	    {{{guidApiTest}}} = {{{guidFolderApi}}}
            	    {{{guidDomainTest}}} = {{{guidFolderDomain}}}
            	    {{{guidInfrastructureTest}}} = {{{guidFolderInfrastructure}}}
                EndGlobalSection
            EndGlobal
            """);

            return (w, $"{tempFilePath}/");

        }

        public static void GenerateCsProj(string tempFilePath, string fileName)
        {
            Log.Debug($"Generating ~ CsProj");
            (InitCsProjWeb(fileName), $"{tempFilePath}/src/Api/")
                .SaveToFile();

            (InitCsProjDomain(), $"{tempFilePath}/src/Domain/")
                .SaveToFile();

            (InitCsProjInfrastructure(), $"{tempFilePath}/src/Infrastructure/")
                .SaveToFile();

            (InitCsProjTest(Layers["Api.Test"]), $"{tempFilePath}/test/UnitTest/Api/").
                SaveToFile();

            (InitCsProjTest(Layers["Domain.Test"]), $"{tempFilePath}/test/UnitTest/Domain/").
                SaveToFile();

            (InitCsProjTest(Layers["Infrastructure.Test"]), $"{tempFilePath}/test/UnitTest/Infrastructure/").
                SaveToFile();

        }

        private static ICodegenOutputFile InitCsProjWeb(string fileName)
        {
            var ctx = new CodegenContext();
            var w = ctx[$"{Layers["Api"]}.csproj"];

            w.WriteLine($$"""
            <Project Sdk="Microsoft.NET.Sdk.Web">
            	<PropertyGroup>
            		<TargetFramework>{{TargetFramework}}</TargetFramework>
            		<ImplicitUsings>enable</ImplicitUsings>
            		<Nullable>enable</Nullable>
                    <StaticWebAssetsEnabled>false</StaticWebAssetsEnabled>
            	</PropertyGroup>
            	<ItemGroup>
            		<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version ="{{NugetVersions["Microsoft.EntityFrameworkCore.InMemory"]}}" />
            		<PackageReference Include="Swashbuckle.AspNetCore" Version="{{NugetVersions["Swashbuckle.AspNetCore"]}}" />
            		<PackageReference Include="Serilog" Version="{{NugetVersions["Serilog"]}}" />
            		<PackageReference Include="Serilog.Sinks.Console" Version ="{{NugetVersions["Serilog.Sinks.Console"]}}" />
            		<PackageReference Include="Serilog.AspNetCore" Version ="{{NugetVersions["Serilog.AspNetCore"]}}" />
            		<PackageReference Include="AutoMapper" Version ="{{NugetVersions["AutoMapper"]}}" />
            		<PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" Version="{{NugetVersions["Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore"]}}" />
            	</ItemGroup>
            	<ItemGroup>
            		<ProjectReference Include="..\Domain\{{Layers["Domain"]}}.csproj" />
            		<ProjectReference Include="..\Infrastructure\{{Layers["Infrastructure"]}}.csproj" />
            	</ItemGroup>
                <ItemGroup>
                  <Content Update="wwwroot\swagger\{{fileName}}">
                    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
                  </Content>
                </ItemGroup>
            </Project>
            """);

            return w;
        }

        private static ICodegenOutputFile InitCsProjInfrastructure()
        {
            var ctx = new CodegenContext();
            var w = ctx[$"{Layers["Infrastructure"]}.csproj"];

            w.WriteLine($$"""
             <Project Sdk="Microsoft.NET.Sdk">
            	<PropertyGroup>
            		<TargetFramework>{{TargetFramework}}</TargetFramework>
            		<ImplicitUsings>enable</ImplicitUsings>
            		<Nullable>enable</Nullable>
            	</PropertyGroup>
            	<ItemGroup>
            		<PackageReference Include="Microsoft.EntityFrameworkCore" Version ="{{NugetVersions["Microsoft.EntityFrameworkCore"]}}" />
            		<PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version ="{{NugetVersions["Microsoft.EntityFrameworkCore.Relational"]}}" />
            	</ItemGroup>
            </Project>
            """);

            return w;
        }

        private static ICodegenOutputFile InitCsProjDomain()
        {
            var ctx = new CodegenContext();
            var w = ctx[$"{Layers["Domain"]}.csproj"];

            w.WriteLine($$"""
            <Project Sdk="Microsoft.NET.Sdk">
            	<PropertyGroup>
            		<TargetFramework>{{TargetFramework}}</TargetFramework>
            		<ImplicitUsings>enable</ImplicitUsings>
            		<Nullable>enable</Nullable>
            	</PropertyGroup>
            	<ItemGroup>
            		<ProjectReference Include="..\Infrastructure\{{Layers["Infrastructure"]}}.csproj" />
            		<PackageReference Include="Microsoft.EntityFrameworkCore.DynamicLinq" Version ="{{NugetVersions["Microsoft.EntityFrameworkCore.DynamicLinq"]}}" />
            		<PackageReference Include="Serilog" Version="{{NugetVersions["Serilog"]}}" />
            	</ItemGroup>
            </Project>
            """);

            return w;
        }

        private static ICodegenOutputFile InitCsProjTest(string csprojName)
        {
            var ctx = new CodegenContext();
            var w = ctx[$"{csprojName}.csproj"];

            w.WriteLine($$"""
            <Project Sdk="Microsoft.NET.Sdk">
            	<PropertyGroup>
            		<TargetFramework>{{TargetFramework}}</TargetFramework>
            		<ImplicitUsings>enable</ImplicitUsings>
            		<Nullable>enable</Nullable>
            	</PropertyGroup>
            	<ItemGroup>
            		<PackageReference Include="Moq" Version="{{NugetVersions["Moq"]}}" />
            		<PackageReference Include="Serilog" Version="{{NugetVersions["Serilog"]}}" />
            		<PackageReference Include="xunit" Version="{{NugetVersions["xunit"]}}" />
            		<PackageReference Include="xunit.runner.visualstudio" Version="{{NugetVersions["xunit.runner.visualstudio"]}}" />
            		<PackageReference Include="coverlet.collector" Version="{{NugetVersions["coverlet.collector"]}}" />
            		<PackageReference Include="Microsoft.NET.Test.Sdk" Version="{{NugetVersions["Microsoft.NET.Test.Sdk"]}}" />
                    <PackageReference Include="MockQueryable.Moq" Version="{{NugetVersions["MockQueryable.Moq"]}}" />
            	</ItemGroup>
            	<ItemGroup>
            		<ProjectReference Include="..\..\..\src\Api\{{Layers["Api"]}}.csproj" />
            		<ProjectReference Include="..\..\..\src\Domain\{{Layers["Domain"]}}.csproj" />
            		<ProjectReference Include="..\..\..\src\Infrastructure\{{Layers["Infrastructure"]}}.csproj" />
            	</ItemGroup>
            </Project>
            """);

            return w;
        }

        public static (ICodegenOutputFile, string?) InitProgram(string tempFilePath, string projectName, string fileName)
        {
            var ctx = new CodegenContext();
            var w = ctx[$"Program.cs"];

            w.WriteLine($$"""
            using Microsoft.EntityFrameworkCore;
            using System.Text.Json.Serialization;
            using AutoMapper;
            using Context;
            using Helpers;
            using Serilog;

            namespace Api
            {
                public static class Program
                {
                    public static void Main(string[] args)
                    {

                        var databaseUrlConnection = Environment.GetEnvironmentVariable("DATABASE_URL")
                            ?? throw new InvalidOperationException($"DATABASE_URL is not set");

                        var builder = WebApplication.CreateBuilder(args);

                        builder.Services.AddDbContext<ApiDbContext>(opt => opt.UseInMemoryDatabase(databaseName: databaseUrlConnection), ServiceLifetime.Scoped, ServiceLifetime.Scoped);

                        builder.Services
                            .AddControllers(o => o.Filters.Add(new HttpResponseExceptionFilter()))
                            .AddJsonOptions(o => o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)
                            .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles)
                            .ConfigureApiBehaviorOptions(o => { o.SuppressModelStateInvalidFilter = true; });

                        builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
                            loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

                        builder.Services.AddRouting(options => options.LowercaseUrls = true);
                        builder.Services.AddEndpointsApiExplorer();
                        builder.Services.AddSwaggerGen();
                        var mapperConfig = new MapperConfiguration(mc => { mc.AddProfile(new MappingProfile()); });
                        builder.Services.AddSingleton(mapperConfig.CreateMapper());
                        builder.Services.AddRepositories();
                        builder.Services.AddServices();
                        builder.Services.AddHealthChecks().AddDbContextCheck<ApiDbContext>();

                        var app = builder.Build();
                        app.UseSerilogRequestLogging();
                        app.UseMiddleware<RequestContextLoggingMiddleware>();
                        app.UseStaticFiles(new StaticFileOptions { ServeUnknownFileTypes = true });
                        app.UseDeveloperExceptionPage();
                        app.UseSwagger();
                        //app.UseSwaggerUI(c => c.SwaggerEndpoint("{{fileName}}", "{{projectName}}"));
                        app.UseSwaggerUI();
                        app.MapControllers();
                        app.MapHealthChecks("/healthcheck");
                        app.Run();
                    }
                }
            }
            
            """);

            return (w, $"{tempFilePath}/src/Api/");
        }

    }
}
