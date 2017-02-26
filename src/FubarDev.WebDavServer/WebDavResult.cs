// <copyright file="WebDavResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// The result of a WebDAV operation
    /// </summary>
    public class WebDavResult : IWebDavResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavResult"/> class.
        /// </summary>
        /// <param name="statusCode">The WebDAV status code</param>
        public WebDavResult(WebDavStatusCode statusCode)
        {
            StatusCode = statusCode;
        }

        /// <inheritdoc />
        public WebDavStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the header values to be set for the response
        /// </summary>
        public IDictionary<string, string[]> Headers { get; } = new Dictionary<string, string[]>();

        /// <inheritdoc />
        public virtual Task ExecuteResultAsync(IWebDavResponse response, CancellationToken ct)
        {
            var headers = new Dictionary<string, string[]>()
            {
                ["DAV"] = response.Dispatcher.SupportedClasses.ToArray(),
            };

            foreach (var header in Headers)
            {
                headers[header.Key] = header.Value;
            }

            foreach (var header in headers)
            {
                response.Headers[header.Key] = header.Value;
            }

            return Task.FromResult(0);
        }
    }
}
