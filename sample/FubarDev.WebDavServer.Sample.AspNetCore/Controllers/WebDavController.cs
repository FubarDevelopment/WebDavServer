using FubarDev.WebDavServer.AspNetCore;
using FubarDev.WebDavServer.AspNetCore.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Sample.AspNetCore.Controllers
{
    [Route("_dav/{*path}")]
    [Authorize]
    [WebDavAnyExceptionFilter]
    public class WebDavController : WebDavControllerBase
    {
        public WebDavController(
            IWebDavContext context,
            IWebDavDispatcher dispatcher,
            ILoggerFactory loggerFactory = null)
            : base(context, dispatcher, loggerFactory)
        {
        }
    }
}
