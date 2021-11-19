// <copyright file="RequestLogMiddleware.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.AspNetCore.Logging
{
    /// <summary>
    /// The request log middleware.
    /// </summary>
    public class RequestLogMiddleware
    {
        private static readonly IEnumerable<MediaType> _xmlMediaTypes = new[]
        {
            "text/xml",
            "application/xml",
            "text/plain",
        }.Select(x => new MediaType(x)).ToList();

        private static readonly Encoding _defaultEncoding = new UTF8Encoding(false);
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLogMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestLogMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware.</param>
        /// <param name="logger">The logger for this middleware.</param>
        public RequestLogMiddleware(RequestDelegate next, ILogger<RequestLogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Tests if the media type qualifies for XML deserialization.
        /// </summary>
        /// <param name="mediaType">The media type to test.</param>
        /// <returns><see langword="true"/> when the media type might be an XML type.</returns>
        public static bool IsXml(string mediaType)
        {
            var contentType = new MediaType(mediaType);
            var isXml = _xmlMediaTypes.Any(x => contentType.IsSubsetOf(x));
            return isXml;
        }

        /// <summary>
        /// Tests if the media type qualifies for XML deserialization.
        /// </summary>
        /// <param name="mediaType">The media type to test.</param>
        /// <returns><see langword="true"/> when the media type might be an XML type.</returns>
        public static bool IsXml(MediaType mediaType)
        {
            var isXml = _xmlMediaTypes.Any(mediaType.IsSubsetOf);
            return isXml;
        }

        /// <summary>
        /// Invoked by ASP.NET core.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>The async task.</returns>
        // ReSharper disable once ConsiderUsingAsyncSuffix
        public async Task Invoke(HttpContext context)
        {
            using (_logger.BeginScope("RequestInfo"))
            {
                var info = new List<string>()
                {
                    $"{context.Request.Protocol} {context.Request.Method} {context.Request.GetDisplayUrl()}",
                };

                try
                {
                    info.AddRange(context.Request.Headers.Select(x => $"{x.Key}: {x.Value}"));
                }
                catch
                {
                    // Ignore all exceptions
                }

                var shouldTryReadingBody =
                    IsXmlContentType(context.Request)
                    || (context.Request.Body != null && IsMicrosoftWebDavClient(context.Request));

                if (shouldTryReadingBody && context.Request.Body != null)
                {
                    var encoding = GetEncoding(context.Request);

                    var temp = new MemoryStream();
                    await context.Request.Body.CopyToAsync(temp, 65536).ConfigureAwait(false);

                    if (temp.Length != 0)
                    {
                        temp.Position = 0;

                        bool showBody;
                        if (HttpMethods.IsPut(context.Request.Method))
                        {
                            showBody = true;
                        }
                        else
                        {
                            try
                            {
                                using (var reader = new StreamReader(temp, encoding, false, 1000, true))
                                {
                                    var doc = XDocument.Load(reader);
                                    info.Add($"Body: {doc}");
                                }

                                showBody = false;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(EventIds.Unspecified, ex, "Failed to read the request body as XML");
                                showBody = true;
                            }
                        }

                        if (showBody)
                        {
                            temp.Position = 0;
                            using var reader = new StreamReader(temp, encoding, false, 1000, true);
                            var content = await reader.ReadToEndAsync().ConfigureAwait(false);
                            info.Add($"Body: {content}");
                        }

                        if (!context.Request.Body.CanSeek)
                        {
                            var oldStream = context.Request.Body;
                            context.Request.Body = temp;
                            await oldStream.DisposeAsync();
                        }

                        context.Request.Body.Position = 0;
                    }
                }

                _logger.LogInformation("Request information: {Information}", string.Join("\r\n", info));
            }

            await _next(context).ConfigureAwait(false);
        }

        private static bool IsXmlContentType(HttpRequest request)
        {
            return request.Body != null
                   && !string.IsNullOrEmpty(request.ContentType)
                   && IsXml(request.ContentType);
        }

        private static bool IsMicrosoftWebDavClient(HttpRequest request)
        {
            if (!request.Headers.TryGetValue("User-Agent", out var userAgentValues))
            {
                return false;
            }

            if (userAgentValues.Count == 0)
            {
                return false;
            }

            return userAgentValues[0].IndexOf("Microsoft-WebDAV-MiniRedir", StringComparison.OrdinalIgnoreCase) != -1;
        }

        private static Encoding GetEncoding(HttpRequest request)
        {
            if (string.IsNullOrEmpty(request.ContentType))
            {
                return _defaultEncoding;
            }

            var contentType = new MediaType(request.ContentType);
            if (contentType.Charset.HasValue)
            {
                return Encoding.GetEncoding(contentType.Charset.Value);
            }

            return _defaultEncoding;
        }
    }
}
