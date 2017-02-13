// <copyright file="WebDavResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Globalization;
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

        public virtual Task ExecuteResultAsync(IWebDavResponse response, CancellationToken ct)
        {
            response.Headers["DAV"] = response.Dispatcher.SupportedClasses.ToArray();
            return Task.FromResult(0);
        }
    }
}
