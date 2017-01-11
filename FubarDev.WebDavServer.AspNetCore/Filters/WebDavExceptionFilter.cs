using System;

using FubarDev.WebDavServer.Model;

using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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
                context.Result = new StatusCodeResult((int)WebDavStatusCodes.NotImplemented);
            }
            else if (context.Exception is WebDavException)
            {
                var ex = (WebDavException) context.Exception;
                var result = new WebDavResult<Multistatus>(
                    ex.StatusCode,
                    new Multistatus()
                    {
                        Response = new[]
                        {
                            new Response()
                            {
                                Href = context.HttpContext.Request.GetEncodedUrl(),
                                ItemsElementName = new[] {ItemsChoiceType1.Status,},
                                Items = new object[] {$"{(int)ex.StatusCode} {ex.Message}"},
                            },
                        }
                    });
                context.Result = new WebDavResultResult<Multistatus>(result);
            }

            _logger.LogError(Logging.EventIds.Unspecified, context.Exception, context.Exception.Message);
        }
    }
}
