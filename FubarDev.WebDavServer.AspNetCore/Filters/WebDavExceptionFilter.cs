using System;

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
                context.Result = new StatusCodeResult((int)Model.WebDavStatusCodes.NotImplemented);
            }

            _logger.LogError(Logging.EventIds.Unspecified, context.Exception, context.Exception.Message);
        }
    }
}
