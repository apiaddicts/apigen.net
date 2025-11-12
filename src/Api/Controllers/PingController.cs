using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Api.Controllers
{

    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    [Route("")]
    public class PingController : ControllerBase
    {
        public static readonly string DotNetVersion = $"{Environment.Version}";
        public static readonly Version? AppVersion = Assembly.GetExecutingAssembly().GetName().Version;
        private static readonly string AppName = $"ApiGen {AppVersion} ~ dotnet {DotNetVersion}";

        public string Index()
        {
            return $"{AppName}";
        }
    }
}
