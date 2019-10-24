// <copyright file="WebDavResult{T}.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// Gets the WebDAV result with a value to be returned in the response body.
    /// </summary>
    /// <typeparam name="T">The type of the value to be serialized as response body.</typeparam>
    public class WebDavResult<T> : WebDavResult
        where T : class, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavResult{T}"/> class.
        /// </summary>
        /// <param name="statusCode">The WebDAV status code.</param>
        /// <param name="data">The data to be returned in the response body.</param>
        public WebDavResult(WebDavStatusCode statusCode, T data)
            : base(statusCode)
        {
            Data = data;
        }

        /// <summary>
        /// Gets the data to be returned in the response body.
        /// </summary>
        public T Data { get; }

        /// <inheritdoc />
        public override async Task ExecuteResultAsync(IWebDavResponse response, CancellationToken ct)
        {
            var formatter = response.Dispatcher.Formatter;
            response.ContentType = formatter.ContentType;
            await base.ExecuteResultAsync(response, ct).ConfigureAwait(false);
            await formatter.SerializeAsync(response.Body, Data, ct).ConfigureAwait(false);
        }
    }
}
