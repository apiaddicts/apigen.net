using CodegenCS;
using Domain.Enums;
using Domain.Utils;
using Serilog;
using static Domain.Utils.StringUtils;

namespace Domain.Generators
{
    class NuGet
    {
        public NuGet(string Name, string Version)
        {
            this.Name = Name;
            this.Version = Version;
        }

        public string Name { get; set; }
        public string Version { get; set; }
    }
    public static class StructureGenerator
    {
        private static readonly string EntityFrameworkCoreVersion = "7.0.5";


        private static readonly string TargetFramework = "net7.0";
        private static readonly NuGet VisualStudioVersion = new("VisualStudioVersion", "17.0.32126.317");
        private static readonly NuGet MinimumVisualStudioVersion = new("MinimumVisualStudioVersion", "10.0.40219.1");
        private static readonly NuGet VisualStudioFormatVersion = new("Microsoft Visual Studio Solution File, Format Version", "12.00");

        private static readonly NuGet Swashbuckle = new("Swashbuckle.AspNetCore", "6.5.0");
        private static readonly NuGet Swashbuckle_Filters = new("Swashbuckle.AspNetCore.Filters", "7.0.6");
        private static readonly NuGet Serilog = new("Serilog", "2.12.0");
        private static readonly NuGet Serilog_Console = new("Serilog.Sinks.Console", "4.1.0");
        private static readonly NuGet AutoMapper = new("AutoMapper", "12.0.1");

        private static readonly NuGet EntityFrameworkCore = new("Microsoft.EntityFrameworkCore", EntityFrameworkCoreVersion);
        private static readonly NuGet EntityFrameworkCore_Relational = new("Microsoft.EntityFrameworkCore.Relational", EntityFrameworkCoreVersion);
        private static readonly NuGet EntityFrameworkCore_DynamicLinq = new("Microsoft.EntityFrameworkCore.DynamicLinq", "7.3.2");
        private static readonly NuGet EntityFrameworkCore_InMemory = new("Microsoft.EntityFrameworkCore.InMemory", EntityFrameworkCoreVersion);
        private static readonly NuGet EntityFrameworkCore_PostgreSQL = new("Npgsql.EntityFrameworkCore.PostgreSQL", "7.0.1");
        private static readonly NuGet EntityFrameworkCore_HealthChecks = new("Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore", EntityFrameworkCoreVersion);

        private static readonly NuGet Moq = new("Moq", "4.18.4");
        private static readonly NuGet XUnit = new("xunit", "2.4.2");
        private static readonly NuGet XUnit_Runner = new("xunit.runner.visualstudio", "2.4.5");

        private static readonly NuGet Coverlet = new("coverlet.collector", "6.0.0");
        private static readonly NuGet Coverlet_Build = new("coverlet.msbuild", "6.0.0");
        private static readonly NuGet Test_SDK = new("Microsoft.NET.Test.Sdk", "17.6.0");


        public static void Generator(string tempFilePath, string projectName, string projectId, string fileName, DatabaseType databaseType = DatabaseType.MEMORY )
        {
            Log.Debug($"Structure Generator: {projectName}: {projectId}");
            GenerateSln(tempFilePath, projectName, projectId).SaveToFile();
            GenerateCsProj(tempFilePath, fileName, databaseType);
            InitProgram(tempFilePath, projectName, fileName, databaseType).SaveToFile();
        }

        public static (ICodegenOutputFile, string?) GenerateSln(string tempFilePath, string projectName, string projectGuid)
        {
            Log.Debug($"Generate Sln: {projectGuid}");
            var ctx = new CodegenContext();
            var w = ctx[$"{projectName}.sln"];
            w.WriteLine($"\n{VisualStudioFormatVersion.Name} {VisualStudioFormatVersion.Version}\n" +
                $"{VisualStudioVersion.Name} = {VisualStudioVersion.Version}\n" +
                $"{MinimumVisualStudioVersion.Name} = {MinimumVisualStudioVersion.Version}");
            w.WriteLine($"Project(\"{{{projectGuid}}}\") = \"Api\", \"Api\\Api.csproj\", \"{{{GuidId()}}}\"\nEndProject");
            w.WriteLine($"Project(\"{{{projectGuid}}}\") = \"Domain\", \"Domain\\Domain.csproj\", \"{{{GuidId()}}}\"\nEndProject");
            w.WriteLine($"Project(\"{{{projectGuid}}}\") = \"Infrastructure\", \"Infrastructure\\Infrastructure.csproj\", \"{{{GuidId()}}}\"\nEndProject");
            w.WriteLine($"Project(\"{{{projectGuid}}}\") = \"Doc\", \"Doc\\Doc.csproj\", \"{{{GuidId()}}}\"\nEndProject");
            w.WriteLine($"Project(\"{{{projectGuid}}}\") = \"Test\", \"Test\\Test.csproj\", \"{{{GuidId()}}}\"\nEndProject");
            return (w, $"{tempFilePath}/");

        }

        public static void GenerateCsProj(string tempFilePath, string fileName, DatabaseType databaseType)
        {
            Log.Debug($"Generate CsProj");
            (InitCsProjWeb(fileName, databaseType), $"{tempFilePath}/Api/")
                .SaveToFile();

            (InitCsProjDomain(), $"{tempFilePath}/Domain/")
                .SaveToFile();

            (InitCsProjInfrastructure(), $"{tempFilePath}/Infrastructure/")
                .SaveToFile();

            (InitCsProj(), $"{tempFilePath}/Doc/")
                .SaveToFile();

            (InitCsProjTest(), $"{tempFilePath}/Test/").
                SaveToFile();

        }

        private static ICodegenOutputFile InitCsProjWeb(string fileName, DatabaseType databaseType)
        {
            var ctx = new CodegenContext();
            var w = ctx[$"Api.csproj"];

            w.WriteLine("<Project Sdk=\"Microsoft.NET.Sdk.Web\">");
            w.WriteLine("\t<PropertyGroup>");
            w.WriteLine($"\t\t<TargetFramework>{TargetFramework}</TargetFramework>");
            w.WriteLine("\t\t<ImplicitUsings>enable</ImplicitUsings>");
            w.WriteLine("\t\t<Nullable>enable</Nullable>");
            w.WriteLine("\t</PropertyGroup>\n");

            w.WriteLine("<ItemGroup>");

            if (databaseType.Equals(DatabaseType.POSTGRESQL))
                w.WriteLine($"\t<PackageReference Include=\"{EntityFrameworkCore_PostgreSQL.Name}\" Version =\"{EntityFrameworkCore_PostgreSQL.Version}\" />");
            else
                w.WriteLine($"\t<PackageReference Include=\"{EntityFrameworkCore_InMemory.Name}\" Version =\"{EntityFrameworkCore_InMemory.Version}\" />");

            w.WriteLine($"\t<PackageReference Include=\"{Swashbuckle.Name}\" Version=\"{Swashbuckle.Version}\" />");
            w.WriteLine($"\t<PackageReference Include=\"{Swashbuckle_Filters.Name}\" Version =\"{Swashbuckle_Filters.Version}\" />");
            w.WriteLine($"\t<PackageReference Include=\"{Serilog.Name}\" Version=\"{Serilog.Version}\" />");
            w.WriteLine($"\t<PackageReference Include=\"{Serilog_Console.Name}\" Version =\"{Serilog_Console.Version}\" />");
            w.WriteLine($"\t<PackageReference Include=\"{AutoMapper.Name}\" Version =\"{AutoMapper.Version}\" />");
            w.WriteLine($"\t<PackageReference Include=\"{EntityFrameworkCore_HealthChecks.Name}\" Version=\"{EntityFrameworkCore_HealthChecks.Version}\" />");
            w.WriteLine($"</ItemGroup>\n");

            w.WriteLine("<ItemGroup>");
            w.WriteLine("\t<ProjectReference Include=\"..\\Domain\\Domain.csproj\" />");
            w.WriteLine("\t<ProjectReference Include=\"..\\Infrastructure\\Infrastructure.csproj\" />");
            w.WriteLine("</ItemGroup>");

            w.WriteLine("<ItemGroup>");
            w.WriteLine($"\t <None Include=\"wwwroot\\swagger\\{fileName}\" />");
            w.WriteLine("</ItemGroup>");

            w.WriteLine("</Project>\n");
            return w;
        }

        private static ICodegenOutputFile InitCsProj()
        {
            var ctx = new CodegenContext();
            var w = ctx[$"Doc.csproj"];
            w.WriteLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
            w.WriteLine("\t<PropertyGroup>");
            w.WriteLine($"\t\t<TargetFramework>{TargetFramework}</TargetFramework>");
            w.WriteLine("\t\t<ImplicitUsings>enable</ImplicitUsings>");
            w.WriteLine("\t\t<Nullable>enable</Nullable>");
            w.WriteLine("\t</PropertyGroup>");
            w.WriteLine("</Project>\n");
            return w;
        }

        private static ICodegenOutputFile InitCsProjInfrastructure()
        {
            var ctx = new CodegenContext();
            var w = ctx[$"Infrastructure.csproj"];

            w.WriteLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
            w.WriteLine("\t<PropertyGroup>");
            w.WriteLine($"\t\t<TargetFramework>{TargetFramework}</TargetFramework>");
            w.WriteLine("\t\t<ImplicitUsings>enable</ImplicitUsings>");
            w.WriteLine("\t\t<Nullable>enable</Nullable>");
            w.WriteLine("\t</PropertyGroup>");

            w.WriteLine("<ItemGroup>");
            w.WriteLine($"\t<PackageReference Include=\"{EntityFrameworkCore.Name}\" Version =\"{EntityFrameworkCore.Version}\" />");
            w.WriteLine($"\t<PackageReference Include=\"{EntityFrameworkCore_Relational.Name}\" Version =\"{EntityFrameworkCore_Relational.Version}\" />");
            w.WriteLine("</ItemGroup>\n");

            w.WriteLine("</Project>\n");
            return w;
        }

        private static ICodegenOutputFile InitCsProjDomain()
        {
            var ctx = new CodegenContext();
            var w = ctx[$"Domain.csproj"];
            w.WriteLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
            w.WriteLine("\t<PropertyGroup>");
            w.WriteLine($"\t\t<TargetFramework>{TargetFramework}</TargetFramework>");
            w.WriteLine("\t\t<ImplicitUsings>enable</ImplicitUsings>");
            w.WriteLine("\t\t<Nullable>enable</Nullable>");
            w.WriteLine("\t</PropertyGroup>");

            w.WriteLine("<ItemGroup>");
            w.WriteLine($"\t<ProjectReference Include=\"..\\Infrastructure\\Infrastructure.csproj\" />");
            w.WriteLine($"\t<PackageReference Include=\"{EntityFrameworkCore_DynamicLinq.Name}\" Version =\"{EntityFrameworkCore_DynamicLinq.Version}\" />");
            w.WriteLine("</ItemGroup>\n");

            w.WriteLine("</Project>\n");
            return w;
        }

        private static ICodegenOutputFile InitCsProjTest()
        {
            var ctx = new CodegenContext();
            var w = ctx[$"Test.csproj"];
            w.WriteLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
            w.WriteLine("\t<PropertyGroup>");
            w.WriteLine($"\t\t<TargetFramework>{TargetFramework}</TargetFramework>");
            w.WriteLine("\t\t<ImplicitUsings>enable</ImplicitUsings>");
            w.WriteLine("\t\t<Nullable>enable</Nullable>");
            w.WriteLine("\t</PropertyGroup>");

            w.WriteLine("<ItemGroup>");
            w.WriteLine($"\t<PackageReference Include = \"{Moq.Name}\" Version = \"{Moq.Version}\"/>");
            w.WriteLine($"\t<PackageReference Include = \"{Serilog.Name}\" Version = \"{Serilog.Version}\"/>");
            w.WriteLine($"\t<PackageReference Include = \"{XUnit.Name}\" Version = \"{XUnit.Version}\"/>");
            w.WriteLine($"\t<PackageReference Include = \"{XUnit_Runner.Name}\" Version = \"{XUnit_Runner.Version}\"/>");
            w.WriteLine($"\t<PackageReference Include=\"{Coverlet.Name}\" Version=\"{Coverlet.Version}\"/>");
            w.WriteLine($"\t<PackageReference Include=\"{Test_SDK.Name}\" Version=\"{Test_SDK.Version}\"/>");
            w.WriteLine($"\t<PackageReference Include=\"{Coverlet_Build.Name}\" Version=\"{Coverlet_Build.Version}\"/>");
            w.WriteLine("</ItemGroup>\n");

            w.WriteLine("<ItemGroup>");
            w.WriteLine("\t<ProjectReference Include=\"..\\Api\\Api.csproj\" />");
            w.WriteLine("\t<ProjectReference Include=\"..\\Domain\\Domain.csproj\" />");
            w.WriteLine("\t<ProjectReference Include=\"..\\Infrastructure\\Infrastructure.csproj\" />");
            w.WriteLine("</ItemGroup>");

            w.WriteLine("</Project>\n");
            return w;
        }



        public static (ICodegenOutputFile, string?) InitProgram(string tempFilePath, string projectName, string fileName, DatabaseType databaseType)
        {
            var ctx = new CodegenContext();
            var w = ctx[$"Program.cs"];
            w.WriteLine("using Swashbuckle.AspNetCore.Filters;");
            w.WriteLine("using Microsoft.OpenApi.Models;");
            w.WriteLine("using Microsoft.EntityFrameworkCore;");
            w.WriteLine("using System.Text.Json.Serialization;");
            w.WriteLine("using AutoMapper;");
            w.WriteLine("using Context;");
            w.WriteLine("using Helpers;");
            w.WriteLine("using Serilog;\n");
            w.WriteLine("var dBConnection =  Environment.GetEnvironmentVariable(\"dBConnection\") ?? \"db\";");
            w.WriteLine("var builder = WebApplication.CreateBuilder(args);");
            w.WriteLine($"var openApiInfo = new OpenApiInfo {{ Title = \"{projectName}\", Version = \"v1\" }};\n");

            if (databaseType.Equals(DatabaseType.POSTGRESQL))
            {
                w.WriteLine("//PostgreSQL\nbuilder.Services.AddDbContext<ApiDbContext>(opt => opt.UseNpgsql(dBConnection));");
                w.WriteLine("AppContext.SetSwitch(\"Npgsql.EnableLegacyTimestampBehavior\", true);");
            }
            else if (databaseType.Equals(DatabaseType.MYSQL))
            {
                w.WriteLine("//MySql\n//builder.Services.AddDbContext<ApiDbContext>(opt => opt.UseMySql(dBConnection));");
            }
            else
            {
                w.WriteLine("//MemoryDatabase\nbuilder.Services.AddDbContext<ApiDbContext>(opt => opt.UseInMemoryDatabase(databaseName: dBConnection), ServiceLifetime.Scoped, ServiceLifetime.Scoped);");
            }

            w.WriteLine("builder.Services" +
                "\n\t.AddControllers(o => o.Filters.Add(new HttpResponseExceptionFilter()))" +
                "\n\t.AddJsonOptions(o => o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)" +
                "\n\t.AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles)" +
                "\n\t.ConfigureApiBehaviorOptions(o => { o.SuppressModelStateInvalidFilter = true; });");

            w.WriteLine("builder.Services.AddRouting(options => options.LowercaseUrls = true);");
            w.WriteLine("builder.Services.AddEndpointsApiExplorer();");
            w.WriteLine("builder.Services.AddSwaggerGen();");
            w.WriteLine("var mapperConfig = new MapperConfiguration(mc =>{mc.AddProfile(new MappingProfile());});");
            w.WriteLine("builder.Services.AddSingleton(mapperConfig.CreateMapper());");

            w.WriteLine("builder.Services.AddRepositories();");
            w.WriteLine("builder.Services.AddServices();");
            w.WriteLine("builder.Services.AddHealthChecks().AddDbContextCheck<ApiDbContext>();");

            w.WriteLine("var app = builder.Build();");

  
            w.WriteLine("app.UseStaticFiles(new StaticFileOptions { ServeUnknownFileTypes = true });");
            w.WriteLine("app.UseDeveloperExceptionPage();");
            w.WriteLine("app.UseSwagger();");
            w.WriteLine($"app.UseSwaggerUI(c => c.SwaggerEndpoint(\"{fileName}\", \"{projectName}\"));");
            w.WriteLine("app.UseSwaggerUI();");

            w.WriteLine("app.MapControllers();");
            w.WriteLine("app.MapHealthChecks(\"/healthcheck\");");
            w.WriteLine("app.Run();");
            return (w, $"{tempFilePath}/Api/");
        }


    }
}
