// <copyright file="WebDavResult{T}.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer
{
    public class WebDavResult<T> : WebDavResult
    {
        public WebDavResult(WebDavStatusCode statusCode, T data)
            : base(statusCode)
        {
            Data = data;
        }

        public T Data { get; }

        public override async Task ExecuteResultAsync(IWebDavResponse response, CancellationToken ct)
        {
            var formatter = response.Dispatcher.Formatter;
            response.ContentType = formatter.ContentType;
            await base.ExecuteResultAsync(response, ct).ConfigureAwait(false);
            formatter.Serialize(response.Body, Data);
        }
    }
}
