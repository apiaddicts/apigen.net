using Microsoft.AspNetCore.Mvc;
using static Api.Helpers.StaticRegistry;

namespace Api.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    [Route("")]
    public class PingController : ControllerBase
    {
        public string Index()
        {
            return $"{AppName}";
        }
    }
}
