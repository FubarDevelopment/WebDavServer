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
    public class WebDavResult : IWebDavResult
    {
        public WebDavResult(WebDavStatusCode statusCode)
        {
            StatusCode = statusCode;
        }

        public WebDavStatusCode StatusCode { get; }

        public IDictionary<string, string[]> Headers { get; } = new Dictionary<string, string[]>();

        public virtual Task ExecuteResultAsync(IWebDavResponse response, CancellationToken ct)
        {
            var headers = new Dictionary<string, string[]>()
            {
                ["DAV"] = response.Dispatcher.SupportedClasses.ToArray(),
                ["Accept-Ranges"] = new[] { "bytes" },
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
