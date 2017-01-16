using FubarDev.WebDavServer.AspNetCore;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FubarDev.WebDavServer.Sample.AspNetCore.Controllers
{
    [Route("{*path}")]
    [Authorize]
    public class WebDavController : WebDavControllerBase
    {
        public WebDavController(IWebDavDispatcher dispatcher)
            : base(dispatcher)
        {
        }
    }
}
