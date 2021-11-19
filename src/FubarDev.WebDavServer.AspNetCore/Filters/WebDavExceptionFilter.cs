// <copyright file="WebDavExceptionFilter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.WebDavServer.Model;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.AspNetCore.Filters
{
    /// <summary>
    /// An exception filter that handles exception of the WebDAV server.
    /// </summary>
    public class WebDavExceptionFilter : IExceptionFilter
    {
        private readonly ILogger? _logger;
        private readonly ILogger<WebDavIndirectResult>? _responseLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavExceptionFilter"/> class.
        /// </summary>
        /// <param name="loggerFactory">The factory for the logger for this exception filter and the <see cref="WebDavIndirectResult"/>.</param>
        public WebDavExceptionFilter(
            ILoggerFactory? loggerFactory = null)
        {
            _logger = loggerFactory?.CreateLogger<WebDavExceptionFilter>();
            _responseLogger = loggerFactory?.CreateLogger<WebDavIndirectResult>();
        }

        /// <inheritdoc />
        public void OnException(ExceptionContext context)
        {
            if (context.ExceptionHandled)
            {
                return;
            }

            if (context.Exception is NotImplementedException || context.Exception is NotSupportedException)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status501NotImplemented);
                return;
            }

            if (context.Exception is WebDavException webDavException)
            {
                context.Result = BuildResultForStatusCode(context, webDavException.StatusCode, webDavException.Message);
                return;
            }

            if (context.Exception is UnauthorizedAccessException unauthorizedAccessException)
            {
                context.Result = BuildResultForStatusCode(context, WebDavStatusCode.Forbidden, unauthorizedAccessException.Message);
                return;
            }

            _logger?.LogError(
                Logging.EventIds.Unspecified,
                context.Exception,
                "An unhandled exception occurred with the message {ExceptionMessage}",
                context.Exception.Message);
        }

        private IActionResult BuildResultForStatusCode(ExceptionContext context, WebDavStatusCode statusCode, string optionalMessge)
        {
            switch (statusCode)
            {
                case WebDavStatusCode.NotModified:
                    // 304 must not return a body
                    return new StatusCodeResult((int)statusCode);
            }

            var result = new WebDavResult<multistatus>(
                statusCode,
                new multistatus()
                {
                    response = new[]
                    {
                        new response()
                        {
                            href = context.HttpContext.Request.GetEncodedUrl(),
                            ItemsElementName = new[] { ItemsChoiceType2.status, },
                            Items = new object[] { new Status(context.HttpContext.Request.Protocol, statusCode, optionalMessge).ToString() },
                        },
                    },
                });
            var dispatcher = context.HttpContext.RequestServices.GetService<IWebDavDispatcher>();
            return new WebDavIndirectResult(dispatcher, result, _responseLogger);
        }
    }
}
