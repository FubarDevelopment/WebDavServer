// <copyright file="WebDavIndirectResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.AspNetCore
{
    /// <summary>
    /// An <see cref="IActionResult"/> implementation that takes a <see cref="IWebDavResult"/>.
    /// </summary>
    public class WebDavIndirectResult : StatusCodeResult
    {
        private static readonly IEnumerable<MediaType> _supportedMediaTypes = new[] { "text/xml", "application/xml" }.Select(x => new MediaType(x)).ToList();

        private readonly IWebDavContext _context;

        private readonly IWebDavResult _result;

        private readonly ILogger<WebDavIndirectResult>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavIndirectResult"/> class.
        /// </summary>
        /// <param name="context">The current WebDAV context.</param>
        /// <param name="result">The result of the WebDAV operation.</param>
        /// <param name="logger">The logger for a <see cref="WebDavIndirectResult"/>.</param>
        public WebDavIndirectResult(IWebDavContext context, IWebDavResult result, ILogger<WebDavIndirectResult>? logger)
            : base((int)result.StatusCode)
        {
            _context = context;
            _result = result;
            _logger = logger;
        }

        /// <inheritdoc />
        public override async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;

            // Register the WebDAV response if it's disposable
            if (_result is IDisposable disposableResult)
            {
                response.RegisterForDispose(disposableResult);
            }

            // Sets the HTTP status code
            await base.ExecuteResultAsync(context).ConfigureAwait(false);

            // Sets the reason phrase
            var responseFeature = context.HttpContext.Features.Get<IHttpResponseFeature>();
            if (responseFeature != null)
            {
                responseFeature.ReasonPhrase = _result.StatusCode.GetReasonPhrase();
            }

            if (_logger?.IsEnabled(LogLevel.Debug) ?? false)
            {
                var loggingResponse = new LoggingWebDavResponse(_context);
                await _result.ExecuteResultAsync(loggingResponse, context.HttpContext.RequestAborted).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(loggingResponse.ContentType))
                {
                    var mediaType = new MediaType(loggingResponse.ContentType);
                    if (_supportedMediaTypes.Any(x => mediaType.IsSubsetOf(x)))
                    {
                        var doc = loggingResponse.Load();
                        if (doc != null)
                        {
                            _logger.LogDebug(
                                "WebDAV Response: {Response}",
                                doc.ToString(SaveOptions.OmitDuplicateNamespaces));
                        }
                    }
                }
            }

            // Writes the XML response
            await _result.ExecuteResultAsync(new WebDavResponse(_context, response), context.HttpContext.RequestAborted).ConfigureAwait(false);
        }
    }
}
