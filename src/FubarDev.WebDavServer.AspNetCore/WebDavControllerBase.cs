// <copyright file="WebDavControllerBase.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

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
        public async Task<IActionResult> QueryOptionsAsync(string path, CancellationToken cancellationToken)
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
        public Task PostAsync(string path, [FromBody] string value, CancellationToken cancellationToken)
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
        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(string path, CancellationToken cancellationToken)
        {
            var result = await _dispatcher.Class1.DeleteAsync(path, cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        [HttpPropFind]
        public async Task<IActionResult> PropFindAsync(
            string path,
            [FromBody] Propfind request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await _dispatcher.Class1.PropFindAsync(path, request, cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        [HttpPropPatch]
        public async Task<IActionResult> PropPatchAsync(string path, [FromBody] Propertyupdate request, CancellationToken cancellationToken)
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
        public async Task<IActionResult> CopyAsync(
            string path,
            [FromHeader(Name = "Destination")] string destination,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await _dispatcher.Class1.CopyAsync(path, new Uri(destination, UriKind.RelativeOrAbsolute), cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        [HttpMove]
        public async Task<IActionResult> MoveAsync(
            string path,
            [FromHeader(Name = "Destination")] string destination,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await _dispatcher.Class1.MoveAsync(path, new Uri(destination, UriKind.RelativeOrAbsolute), cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        [HttpLock]
        public Task<IActionResult> LockAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new WebDavException(WebDavStatusCode.NotImplemented);
        }

        [HttpUnlock]
        public Task<IActionResult> UnlockAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new WebDavException(WebDavStatusCode.NotImplemented);
        }
    }
}
