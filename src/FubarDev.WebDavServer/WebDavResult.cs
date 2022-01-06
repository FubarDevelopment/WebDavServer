// <copyright file="WebDavResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// The result of a WebDAV operation.
    /// </summary>
    public class WebDavResult : IWebDavResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavResult"/> class.
        /// </summary>
        /// <param name="statusCode">The WebDAV status code.</param>
        public WebDavResult(WebDavStatusCode statusCode)
        {
            StatusCode = statusCode;
        }

        /// <inheritdoc />
        public WebDavStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the header values to be set for the response.
        /// </summary>
        public Dictionary<string, IEnumerable<string>> Headers { get; } = new Dictionary<string, IEnumerable<string>>();

        /// <inheritdoc />
        public virtual Task ExecuteResultAsync(IWebDavResponse response, CancellationToken ct)
        {
            IImmutableDictionary<string, IEnumerable<string>> headers = ImmutableDictionary<string, IEnumerable<string>>.Empty;

            foreach (var webDavClass in response.Context.Dispatcher.SupportedClasses)
            {
                headers = AddHeaderValues(headers, webDavClass.DefaultResponseHeaders);
            }

            headers = AddHeaderValues(headers, Headers);

            foreach (var header in headers)
            {
                response.Headers[header.Key] = header.Value.ToArray();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Adds header values to the current header dictionary.
        /// </summary>
        /// <param name="currentHeaders">The current header dictionary.</param>
        /// <param name="headersToAdd">The headers to add.</param>
        /// <returns>The updated header dictionary.</returns>
        protected IImmutableDictionary<string, IEnumerable<string>> AddHeaderValues(
            IImmutableDictionary<string, IEnumerable<string>> currentHeaders,
            IReadOnlyDictionary<string, IEnumerable<string>> headersToAdd)
        {
            foreach (var header in headersToAdd)
            {
                currentHeaders = currentHeaders.SetItem(
                    header.Key,
                    currentHeaders.TryGetValue(header.Key, out var oldValues) ? oldValues.Union(header.Value) : header.Value);
            }

            return currentHeaders;
        }
    }
}
