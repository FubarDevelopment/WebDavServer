// <copyright file="WebDavXmlResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// A WebDAV result returning an <see cref="XElement"/> in the response body.
    /// </summary>
    public class WebDavXmlResult : WebDavResult
    {
        private readonly XElement _element;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavXmlResult"/> class.
        /// </summary>
        /// <param name="statusCode">The WebDAV status code.</param>
        /// <param name="element">The element to be serialized as the response body.</param>
        public WebDavXmlResult(WebDavStatusCode statusCode, XElement element)
            : base(statusCode)
        {
            _element = element;
        }

        /// <inheritdoc />
        public override async Task ExecuteResultAsync(IWebDavResponse response, CancellationToken ct)
        {
            await base.ExecuteResultAsync(response, ct).ConfigureAwait(false);
            await response.Context.Dispatcher.Formatter.SerializeAsync(response.Body, _element, ct).ConfigureAwait(false);
        }
    }
}
