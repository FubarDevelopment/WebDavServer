using FubarDev.WebDavServer.AspNetCore;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Sample.AspNetCore.Controllers
{
    [Route("{*path}")]
    /* [Authorize] */
    public class WebDavController : WebDavControllerBase
    {
        public WebDavController(IWebDavDispatcher dispatcher, ILogger<WebDavIndirectResult> responseLogger = null)
            : base(dispatcher, responseLogger)
        {
        }
    }
}
