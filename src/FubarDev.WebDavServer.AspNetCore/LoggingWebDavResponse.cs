// <copyright file="LoggingWebDavResponse.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.AspNetCore.Logging;
using FubarDev.WebDavServer.Utils;

using Microsoft.AspNetCore.Http;

namespace FubarDev.WebDavServer.AspNetCore
{
    /// <summary>
    /// A <see cref="IWebDavResponse"/> implementation that buffers the output of a <see cref="WebDavResponse"/>.
    /// </summary>
    public class LoggingWebDavResponse : WebDavResponse
    {
        private readonly HttpResponse _response;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingWebDavResponse"/> class.
        /// </summary>
        /// <param name="context">The current WebDAV context.</param>
        /// <param name="response">The ASP.NET Core HTTP response.</param>
        public LoggingWebDavResponse(IWebDavContext context, HttpResponse response)
            : base(context, response)
        {
            _response = response;
        }

        /// <summary>
        /// Gets the buffered output stream
        /// </summary>
        public override Stream Body { get; } = new MemoryStream();

        /// <summary>
        /// Loads the <see cref="Body"/> into a <see cref="XDocument"/>.
        /// </summary>
        /// <returns>The <see cref="XDocument"/> from the <see cref="Body"/>.</returns>
        public XDocument? Load()
        {
            Body.Position = 0;
            if (Body.Length == 0)
            {
                return null;
            }

            if (!RequestLogMiddleware.IsXml(ContentType))
            {
                return null;
            }

            try
            {
                return XDocument.Load(Body);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Writes the buffered output to the http response body
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the http request</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task WriteBufferedOutPutToResponse(CancellationToken cancellationToken)
        {
            Body.Position = 0;
            await Body.CopyToAsync(_response.Body, SystemInfo.CopyBufferSize, cancellationToken);
        }
    }
}
