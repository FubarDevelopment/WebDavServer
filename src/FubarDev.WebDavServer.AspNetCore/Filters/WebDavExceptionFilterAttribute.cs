// <copyright file="WebDavExceptionFilterAttribute.cs" company="Fubar Development Junker">
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
    /// Attribute to handle exception filtering for WebDAV requests.
    /// </summary>
    public class WebDavExceptionFilterAttribute : ExceptionFilterAttribute
    {
        /// <inheritdoc />
        public override void OnException(ExceptionContext context)
        {
            if (context.ExceptionHandled)
            {
                return;
            }

            switch (context.Exception)
            {
                case NotImplementedException:
                case NotSupportedException:
                    context.Result = new StatusCodeResult(StatusCodes.Status501NotImplemented);
                    context.ExceptionHandled = true;
                    return;
                case WebDavException webDavException:
                    context.Result = BuildResultForStatusCode(context, webDavException.StatusCode);
                    context.ExceptionHandled = true;
                    return;
                case UnauthorizedAccessException:
                    context.Result = BuildResultForStatusCode(context, WebDavStatusCode.Forbidden);
                    context.ExceptionHandled = true;
                    return;
            }

            var logger = context.HttpContext.RequestServices.GetService<ILogger<WebDavExceptionFilterAttribute>>();
            logger?.LogError(
                Logging.EventIds.Unspecified,
                context.Exception,
                "An unhandled exception occurred with the message {ExceptionMessage}",
                context.Exception.Message);
        }

        protected static IActionResult BuildResultForStatusCode(
            ExceptionContext context,
            WebDavStatusCode statusCode,
            string? optionalMessage = null)
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
                            Items = new object[]
                            {
                                new Status(
                                    context.HttpContext.Request.Protocol,
                                    statusCode,
                                    optionalMessage ?? context.Exception.Message).ToString(),
                            },
                        },
                    },
                });

            var responseLogger = context.HttpContext.RequestServices.GetService<ILogger<WebDavIndirectResult>>();
            var webDavContext = context.HttpContext.RequestServices.GetRequiredService<IWebDavContext>();
            return new WebDavIndirectResult(webDavContext, result, responseLogger);
        }
    }
}
