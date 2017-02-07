using FubarDev.WebDavServer.AspNetCore;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Tests.Support
{
    [Route("{*path}")]
    /* [Authorize] */
    public class TestWebDavController : WebDavControllerBase
    {
        public TestWebDavController(IWebDavDispatcher dispatcher, ILogger<WebDavIndirectResult> responseLogger = null)
            : base(dispatcher, responseLogger)
        {
        }
    }
}
