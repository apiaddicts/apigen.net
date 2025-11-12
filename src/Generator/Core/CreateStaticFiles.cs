using CodegenCS;
using Generator.Utils;
using Humanizer;
using Serilog;
using static Generator.Utils.FileUtils;

namespace Generator.Core
{
    public static class CreateStaticFiles
    {

        public static void Generate(string tempFilePath, string projectName, string projectDescription)
        {
            Log.Debug($"Adding ~ .gitignore, launchSettings, appsettings, Dockerfile & Readme");
            GenerateGitIgnore(tempFilePath).SaveToFile();
            GenerateLaunchSettings(tempFilePath, projectName).SaveToFile();
            GenerateAppSettings(tempFilePath).SaveToFile();
            GenerateAppSettingsDev(tempFilePath).SaveToFile();
            GenerateDockerfile(tempFilePath, projectName).SaveToFile();
            GenerateReadme(tempFilePath, projectName, projectDescription).SaveToFile();
        }

        public static (ICodegenOutputFile, string?) GenerateLaunchSettings(string tempFilePath, string projectName)
        {
            var ctx = new CodegenContext();
            var w = ctx[$"launchSettings.json"];

            w.WriteLine($$"""
            {
              "profiles": {
                "{{projectName.Dehumanize()}}": {
                  "commandName": "Project",
                  "launchBrowser": true,
                  "launchUrl": "swagger",
                  "environmentVariables": {
                    "ASPNETCORE_ENVIRONMENT": "Development",
                    "DATABASE_URL" : "<DATABASE_URL>"
                  },
                  "applicationUrl": "https://localhost:8000;",
                  "dotnetRunMessages": true
                }
              }
            }
            """);

            return (w, $"{tempFilePath}/src/Api/Properties/");
        }

        public static (ICodegenOutputFile, string?) GenerateAppSettings(string tempFilePath)
        {

            var ctx = new CodegenContext();
            var w = ctx[$"appsettings.json"];

            w.WriteLine($$"""
            {
              "Logging": {
                "LogLevel": {
                  "Default": "Information",
                  "Microsoft.AspNetCore": "Warning"
                }
              },
              "AllowedHosts": "*"
            }
            """);

            return (w, $"{tempFilePath}/src/Api/");
        }

        public static (ICodegenOutputFile, string?) GenerateAppSettingsDev(string tempFilePath)
        {
            var ctx = new CodegenContext();
            var w = ctx[$"appsettings.Development.json"];

            w.WriteLine($$"""
            {
              "Serilog": {
                "Using": [ "Serilog.Sinks.Console" ],
                "MinimumLevel": {
                  "Default": "Debug",
                  "Override": {
                    "Microsoft.EntityFrameworkCore": "Information",
                    "System": "Information",
                    "Microsoft.AspNetCore": "Warning"
                  }
                },
                "WriteTo": [
                  {
                    "Name": "Console",
                    "Args": {
                      "outputTemplate": "[{Timestamp:HH:mm:ss} {CorrelationId} {Level:u3}] {Message:lj}{NewLine}{Exception}"
                    }
                  }
                ],
                "Enrich": [ "FromLogContext" ]
              }
            }
            """);

            return (w, $"{tempFilePath}/src/Api/");
        }

        public static (ICodegenOutputFile, string?) GenerateGitIgnore(string tempFilePath)
        {
            var ctx = new CodegenContext();
            var w = ctx[$".gitignore"];

            w.WriteLine($$"""
            ###################
            # compiled source #
            ###################
            *.com
            *.class
            *.dll
            *.exe
            *.pdb
            *.dll.config
            *.cache
            *.suo
            # Include dlls if they’re in the NuGet packages directory
            !/packages/*/lib/*.dll
            !/packages/*/lib/*/*.dll
            # Include dlls if they're in the CommonReferences directory
            !*CommonReferences/*.dll
            ####################
            # VS Upgrade stuff #
            ####################
            UpgradeLog.XML
            _UpgradeReport_Files/
            ###############
            # Directories #
            ###############
            bin/
            obj/
            TestResults/
            ###################
            # Web publish log #
            ###################
            *.Publish.xml
            #############
            # Resharper #
            #############
            /_ReSharper.*
            *.ReSharper.*
            ############
            # Packages #
            ############
            # it’s better to unpack these files and commit the raw source
            # git has its own built in compression methods
            *.7z
            *.dmg
            *.gz
            *.iso
            *.jar
            *.rar
            *.tar
            ######################
            # Logs and databases #
            ######################
            *.log
            *.sqlite
            # OS generated files #
            ######################
            .DS_Store?
            ehthumbs.db
            Icon?
            Thumbs.db
            [Bb]in
            [Oo]bj
            [Tt]est[Rr]esults
            *.suo
            *.user
            *.[Cc]ache
            *[Rr]esharper*
            packages
            NuGet.exe
            _[Ss]cripts
            *.exe
            *.dll
            *.nupkg
            *.ncrunchsolution
            *.dot[Cc]over
            .vs

            # Profiles
            src/Api/Properties/launchSettings.json

            # Builds
            /build
            """);

            return (w, $"{tempFilePath}/");
        }

        public static (ICodegenOutputFile, string?) GenerateDockerfile(string tempFilePath, string projectName)
        {
            var ctx = new CodegenContext();
            var w = ctx[$"Dockerfile"];

            w.WriteLine($$"""
            FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine3.20

            COPY build/ .

            EXPOSE 8080
            ENTRYPOINT ["dotnet", "{{projectName.Dehumanize()}}.Api.dll"]
            """);

            return (w, $"{tempFilePath}/");
        }

        public static (ICodegenOutputFile, string?) GenerateReadme(string tempFilePath, string projectName, string projectDescription)
        {
            var ctx = new CodegenContext();
            var w = ctx[$"README.md"];

            w.WriteLine($$"""
            # 🍩 {{projectName.Dehumanize()}} ~  ![.Net](https://img.shields.io/badge/8.0-5C2D91?style=flat&logo=.net&logoColor=white) ![OpenApi](https://img.shields.io/badge/OpenApi-6BA539?style=flat&logo=openapiinitiative&logoColor=white) 
            
            {{projectDescription}}

            # 🖥️ Reqs
            - [Visual Studio 2022](https://visualstudio.microsoft.com/es/vs/)
            - [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
            - [Docker](https://www.docker.com/products/docker-desktop/)

            # ▶️ How to start

            ## `cli` dotnet
            ```
            dotnet run --project ./src/Api/Api.csproj
            ```

            ## `build` dotnet
            ```
            dotnet build -o build/
            ```

            ## 🐋 `docker`
            #### ⚠️ *This step requires the build step*
            ```
            docker build -t template .
            docker run -d -p 8080:8080 --name template.api template
            ```

            # ⚙️ Envs

            | ENV                    | ALLOWED_VALUES                   | DESCRIPTION                                | EXAMPLE                                        |
            |------------------------|----------------------------------|--------------------------------------------|------------------------------------------------|
            | ASPNETCORE_ENVIRONMENT | Development, Staging, Production | Asp.Net profile                            | Development                                    |
            | DATABASE_URL           |                                  | Connection data of the PostgreSQL database | User Id=<>;Password=<>;Data Source=<>:1521/<>; |
            """);

            return (w, $"{tempFilePath}/");
        }

    }
}
