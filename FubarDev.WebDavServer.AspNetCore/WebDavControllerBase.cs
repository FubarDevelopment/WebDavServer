using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.AspNetCore.Routing;
using FubarDev.WebDavServer.Model;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.AspNetCore
{
    public class WebDavControllerBase : ControllerBase
    {
        private readonly IWebDavDispatcher _dispatcher;
        private readonly ILogger<WebDavIndirectResult> _responseLogger;

        public WebDavControllerBase(IWebDavDispatcher dispatcher, ILogger<WebDavIndirectResult> responseLogger = null)
        {
            _dispatcher = dispatcher;
            _responseLogger = responseLogger;
        }

        [HttpOptions]
        public  async Task<IActionResult> QueryOptionsAsync(string path, CancellationToken cancellationToken)
        {
            var result = await _dispatcher.Class1.OptionsAsync(path, cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        [HttpMkCol]
        public async Task<IActionResult> MkColAsync(string path, CancellationToken cancellationToken)
        {
            var result = await _dispatcher.Class1.MkColAsync(path, cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(string path, CancellationToken cancellationToken)
        {
            var result = await _dispatcher.Class1.GetAsync(path, cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        // POST api/values
        [HttpPost]
        public Task PostAsync(string path, [FromBody]string value, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        // PUT api/values/5
        [HttpPut]
        public async Task<IActionResult> PutAsync(string path, CancellationToken cancellationToken)
        {
            var result = await _dispatcher.Class1.PutAsync(path, HttpContext.Request.Body, cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        // DELETE api/values/5
        [HttpDelete()]
        public async Task<IActionResult> DeleteAsync(string path, CancellationToken cancellationToken)
        {
            var result = await _dispatcher.Class1.DeleteAsync(path, cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        [HttpPropFind()]
        public async Task<IActionResult> PropFindAsync(string path, [FromBody]Propfind request, [FromHeader(Name = "Depth")] string depth = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var parsedDepth = Depth.Parse(depth);
            var result = await _dispatcher.Class1.PropFindAsync(path, request, parsedDepth, cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        [HttpPropPatch]
        public async Task<IActionResult> PropPatchAsync(string path, [FromBody]Propertyupdate request, CancellationToken cancellationToken)
        {
            var result = await _dispatcher.Class1.PropPatchAsync(path, request, cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        [HttpHead]
        public async Task<IActionResult> HeadAsync(string path, CancellationToken cancellationToken)
        {
            var result = await _dispatcher.Class1.HeadAsync(path, cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        [HttpCopy]
        public async Task<IActionResult> CopyAsync(string path, [FromHeader(Name = "Destination")] string destination, [FromHeader(Name = "Depth")] string depth = null, [FromHeader(Name = "Overwrite")] string overwrite = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var parsedDepth = Depth.Parse(depth);
            var result = await _dispatcher.Class1.CopyAsync(path, new Uri(destination, UriKind.RelativeOrAbsolute), parsedDepth, ParseOverwrite(overwrite), cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        [HttpMove]
        public async Task<IActionResult> MoveAsync(string path, [FromHeader(Name = "Destination")] string destination, [FromHeader(Name = "Depth")] string depth = null, [FromHeader(Name = "Overwrite")] string overwrite = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var parsedDepth = Depth.Parse(depth);
            var result = await _dispatcher.Class1.MoveAsync(path, new Uri(destination, UriKind.RelativeOrAbsolute), parsedDepth, ParseOverwrite(overwrite), cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        private static bool? ParseOverwrite(string overwrite)
        {
            if (string.IsNullOrWhiteSpace(overwrite))
                return null;
            overwrite = overwrite.Trim();
            if (overwrite == "T")
                return true;
            if (overwrite == "F")
                return false;
            throw new NotSupportedException($"Overwrite value '{overwrite}' isn't supported");
        }
    }
}
