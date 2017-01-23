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
    public class WebDavExceptionFilter : IExceptionFilter
    {
        private readonly ILogger _logger;

        public WebDavExceptionFilter(ILogger<WebDavExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.ExceptionHandled)
                return;

            if (context.Exception is NotImplementedException || context.Exception is NotSupportedException)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status501NotImplemented);
                return;
            }

            var webDavException = context.Exception as WebDavException;
            if (webDavException != null)
            {
                context.Result = BuildResultForStatusCode(context, webDavException.StatusCode, webDavException.Message);
                return;
            }

            var unauthorizedAccessException = context.Exception as UnauthorizedAccessException;
            if (unauthorizedAccessException != null)
            {
                context.Result = BuildResultForStatusCode(context, WebDavStatusCodes.Forbidden, unauthorizedAccessException.Message);
                return;
            }

            _logger.LogError(Logging.EventIds.Unspecified, context.Exception, context.Exception.Message);
        }

        private static IActionResult BuildResultForStatusCode(ExceptionContext context, WebDavStatusCodes statusCode, string optionalMessge)
        {
            var result = new WebDavResult<Multistatus>(
                statusCode,
                new Multistatus()
                {
                    Response = new[]
                    {
                            new Response()
                            {
                                Href = context.HttpContext.Request.GetEncodedUrl(),
                                ItemsElementName = new[] {ItemsChoiceType2.Status,},
                                Items = new object[] {$"{context.HttpContext.Request.Protocol} {(int)statusCode} {statusCode.GetReasonPhrase(optionalMessge)}"},
                            },
                    }
                });
            var dispatcher = context.HttpContext.RequestServices.GetService<IWebDavDispatcher>();
            return new WebDavIndirectResult(dispatcher, result);
        }
    }
}
