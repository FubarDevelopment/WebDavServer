using FubarDev.WebDavServer.AspNetCore;

using Microsoft.AspNetCore.Mvc;

namespace FubarDev.WebDavServer.Sample.AspNetCore.Controllers
{
    [Route("{*path}")]
    public class WebDavController : WebDavControllerBase
    {
    }
}
