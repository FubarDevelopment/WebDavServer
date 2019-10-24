// <copyright file="WebDavControllerBase.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.AspNetCore.Routing;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.AspNetCore
{
    /// <summary>
    /// The default WebDAV controller.
    /// </summary>
    public class WebDavControllerBase : ControllerBase
    {
        private readonly IWebDavContext _context;
        private readonly IWebDavDispatcher _dispatcher;
        private readonly ILogger<WebDavIndirectResult>? _responseLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavControllerBase"/> class.
        /// </summary>
        /// <param name="context">The WebDAV request context.</param>
        /// <param name="dispatcher">The WebDAV HTTP method dispatcher.</param>
        /// <param name="responseLogger">The logger for the <see cref="WebDavIndirectResult"/>.</param>
        public WebDavControllerBase(IWebDavContext context, IWebDavDispatcher dispatcher, ILogger<WebDavIndirectResult>? responseLogger = null)
        {
            _context = context;
            _dispatcher = dispatcher;
            _responseLogger = responseLogger;
        }

        /// <summary>
        /// Handler for the <c>QUERY</c> method.
        /// </summary>
        /// <param name="path">The root-relative path to the target of this method.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The action result.</returns>
        [HttpOptions]
        public async Task<IActionResult> QueryOptionsAsync(string path, CancellationToken cancellationToken)
        {
            var result = await _dispatcher.Class1.OptionsAsync(path ?? string.Empty, cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        /// <summary>
        /// Handler for the <c>MKCOL</c> method.
        /// </summary>
        /// <param name="path">The root-relative path to the target of this method.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The action result.</returns>
        [HttpMkCol]
        public async Task<IActionResult> MkColAsync(string path, CancellationToken cancellationToken)
        {
            var result = await _dispatcher.Class1.MkColAsync(path ?? string.Empty, cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        /// <summary>
        /// Handler for the <c>GET</c> method.
        /// </summary>
        /// <param name="path">The root-relative path to the source of this method.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The action result.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync(string path, CancellationToken cancellationToken)
        {
            var result = await _dispatcher.Class1.GetAsync(path ?? string.Empty, cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        /// <summary>
        /// Handler for the <c>PUT</c> method.
        /// </summary>
        /// <param name="path">The root-relative path to the target of this method.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The action result.</returns>
        [HttpPut]
        public async Task<IActionResult> PutAsync(string path, CancellationToken cancellationToken)
        {
            var result = await _dispatcher.Class1.PutAsync(path ?? string.Empty, HttpContext.Request.Body, cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        /// <summary>
        /// Handler for the <c>DELETE</c> method.
        /// </summary>
        /// <param name="path">The root-relative path to the target of this method.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The action result.</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(string path, CancellationToken cancellationToken)
        {
            var result = await _dispatcher.Class1.DeleteAsync(path ?? string.Empty, cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        /// <summary>
        /// Handler for the <c>PROPFIND</c> method.
        /// </summary>
        /// <param name="path">The root-relative path to the target of this method.</param>
        /// <param name="request">The <see cref="propfind"/> request element (may be null).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The action result.</returns>
        [HttpPropFind]
        public async Task<IActionResult> PropFindAsync(
            string path,
            [FromBody] propfind request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await _dispatcher.Class1.PropFindAsync(path ?? string.Empty, request, cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        /// <summary>
        /// Handler for the <c>PROPPATCH</c> method.
        /// </summary>
        /// <param name="path">The root-relative path to the target of this method.</param>
        /// <param name="request">The <see cref="propertyupdate"/> request element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The action result.</returns>
        [HttpPropPatch]
        public async Task<IActionResult> PropPatchAsync(string path, [FromBody] propertyupdate request, CancellationToken cancellationToken)
        {
            var result = await _dispatcher.Class1.PropPatchAsync(path ?? string.Empty, request, cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        /// <summary>
        /// Handler for the <c>HEAD</c> method.
        /// </summary>
        /// <param name="path">The root-relative path to the target of this method.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The action result</returns>
        [HttpHead]
        public async Task<IActionResult> HeadAsync(string path, CancellationToken cancellationToken)
        {
            var result = await _dispatcher.Class1.HeadAsync(path ?? string.Empty, cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        /// <summary>
        /// Handler for the <c>COPY</c> method.
        /// </summary>
        /// <param name="path">The root-relative path to the source of this method.</param>
        /// <param name="destination">The destination of the COPY operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The action result.</returns>
        [HttpCopy]
        public async Task<IActionResult> CopyAsync(
            string path,
            [FromHeader(Name = "Destination")] string destination,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await _dispatcher.Class1.CopyAsync(path ?? string.Empty, new Uri(destination, UriKind.RelativeOrAbsolute), cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        /// <summary>
        /// Handler for the <c>MOVE</c> method.
        /// </summary>
        /// <param name="path">The root-relative path to the source of this method.</param>
        /// <param name="destination">The destination of the MOVE operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The action result.</returns>
        [HttpMove]
        public async Task<IActionResult> MoveAsync(
            string path,
            [FromHeader(Name = "Destination")] string destination,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await _dispatcher.Class1.MoveAsync(path ?? string.Empty, new Uri(destination, UriKind.RelativeOrAbsolute), cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        /// <summary>
        /// Handler for the <c>LOCK</c> method.
        /// </summary>
        /// <param name="path">The root-relative path to the target of this method.</param>
        /// <param name="lockinfo">The information about the requested lock.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The action result.</returns>
        [HttpLock]
        public async Task<IActionResult> LockAsync(
            string path,
            [FromBody] lockinfo lockinfo,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_dispatcher.Class2 == null)
            {
                throw new NotSupportedException();
            }

            IWebDavResult result;
            if (lockinfo == null)
            {
                // Refresh
                var ifHeader = _context.RequestHeaders.If;
                if (ifHeader == null || ifHeader.Lists.Count == 0)
                {
                    return BadRequest();
                }

                var timeoutHeader = _context.RequestHeaders.Timeout;
                result = await _dispatcher.Class2.RefreshLockAsync(path ?? string.Empty, ifHeader, timeoutHeader, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Lock
                result = await _dispatcher.Class2.LockAsync(path ?? string.Empty, lockinfo, cancellationToken).ConfigureAwait(false);
            }

            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }

        /// <summary>
        /// Handler for the <c>UNLOCK</c> method.
        /// </summary>
        /// <param name="path">The root-relative path to the target of this method.</param>
        /// <param name="lockToken">The token of the lock to remove.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The action result.</returns>
        [HttpUnlock]
        public async Task<IActionResult> UnlockAsync(
            string path,
            [FromHeader(Name = "Lock-Token")] string lockToken,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_dispatcher.Class2 == null)
            {
                throw new NotSupportedException();
            }

            if (string.IsNullOrEmpty(lockToken))
            {
                return new WebDavIndirectResult(_dispatcher, new WebDavResult(WebDavStatusCode.BadRequest), _responseLogger);
            }

            var lt = LockTokenHeader.Parse(lockToken);
            var result = await _dispatcher.Class2.UnlockAsync(path ?? string.Empty, lt, cancellationToken).ConfigureAwait(false);
            return new WebDavIndirectResult(_dispatcher, result, _responseLogger);
        }
    }
}
