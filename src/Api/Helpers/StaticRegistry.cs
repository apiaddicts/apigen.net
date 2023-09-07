using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Api.Helpers
{
    public static class StaticRegistry
    {

        public static string DotNetVersion = $"{Environment.Version}";
        public static Version? AppVersion = Assembly.GetExecutingAssembly().GetName().Version;
        public static string CycleVersion = $"Alpha v{AppVersion}";
        public static string AppName = $"ApiGen {CycleVersion} ~ dotnet {DotNetVersion}";
        public static string Title = $"🍩 {AppName}";
        public static string OpenApiVersion = "v1";
        

        public static OpenApiLicense AppLicense = new OpenApiLicense
        {
            Url = new Uri("https://www.gnu.org/licenses/lgpl-3.0.html"),
            Name = "LGPL-3.0 license"
        };

        public static string AppDescription = $"Archetype generator in dotnet with hexagonal architecture based on an openapi document with extended annotations. This project is a wrapper of the java apigen with springboot but using dotnet and adapting some concepts due to the paradigm difference. The project is currently being developed by the CloudAPPi Services.";

        public static OpenApiContact Contact = new OpenApiContact
        {
            Email = "contacta@apiquality.es",
            Url = new Uri("https://apiquality.es/"),
            Name = "ApiQuality"
        };

}
}
