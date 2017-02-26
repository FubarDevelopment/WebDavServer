// <copyright file="LoggingWebDavResponse.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

using FubarDev.WebDavServer.AspNetCore.Logging;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.AspNetCore
{
    /// <summary>
    /// A <see cref="IWebDavResponse"/> implementation that buffers the output of a <see cref="IWebDavResult"/>
    /// </summary>
    public class LoggingWebDavResponse : IWebDavResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingWebDavResponse"/> class.
        /// </summary>
        /// <param name="dispatcher">The dispatcher implementation for the WebDAV server</param>
        public LoggingWebDavResponse(IWebDavDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            ContentType = "text/xml";
        }

        /// <inheritdoc />
        public IWebDavDispatcher Dispatcher { get; }

        /// <inheritdoc />
        public IDictionary<string, string[]> Headers { get; } = new Dictionary<string, string[]>();

        /// <inheritdoc />
        public string ContentType { get; set; }

        /// <inheritdoc />
        public Stream Body { get; } = new MemoryStream();

        /// <summary>
        /// Loads the <see cref="Body"/> into a <see cref="XDocument"/>
        /// </summary>
        /// <returns>The <see cref="XDocument"/> from the <see cref="Body"/></returns>
        [CanBeNull]
        public XDocument Load()
        {
            Body.Position = 0;
            if (Body.Length == 0)
                return null;

            if (!RequestLogMiddleware.IsXml(ContentType))
                return null;

            try
            {
                return XDocument.Load(Body);
            }
            catch
            {
                return null;
            }
        }
    }
}
