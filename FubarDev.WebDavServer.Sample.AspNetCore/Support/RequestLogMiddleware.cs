using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Sample.AspNetCore.Support
{
    public class RequestLogMiddleware
    {
        private readonly IEnumerable<MediaType> _supportedMediaTypes = new[] { "text/xml", "application/xml" }.Select(x => new MediaType(x)).ToList();
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLogMiddleware> _logger;

        public RequestLogMiddleware(RequestDelegate next, ILogger<RequestLogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            using (_logger.BeginScope("RequestInfo"))
            {
                var info = new List<string>()
                    {
                        $"{context.Request.Protocol} {context.Request.Method} {context.Request.GetDisplayUrl()}"
                    };

                try
                {
                    info.AddRange(context.Request.Headers.Select(x => $"{x.Key}: {x.Value}"));
                }
                catch
                {
                    // Ignore all exceptions
                }

                if (context.Request.Body != null && !string.IsNullOrEmpty(context.Request.ContentType))
                {
                    var contentType = new MediaType(context.Request.ContentType);
                    var isXml = _supportedMediaTypes.Any(x => contentType.IsSubsetOf(x));
                    if (isXml)
                    {
                        var temp = new MemoryStream();
                        await context.Request.Body.CopyToAsync(temp, 65536).ConfigureAwait(false);

                        if (temp.Length != 0)
                        {
                            temp.Position = 0;
                            var doc = XDocument.Load(temp);
                            info.Add($"Body: {doc}");

                            if (!context.Request.Body.CanSeek)
                            {
                                var oldStream = context.Request.Body;
                                context.Request.Body = temp;
                                oldStream.Dispose();
                            }

                            context.Request.Body.Position = 0;
                        }
                    }
                }

                _logger.LogInformation(string.Join("\r\n", info));
            }

            await _next(context).ConfigureAwait(false);
        }
    }
}
