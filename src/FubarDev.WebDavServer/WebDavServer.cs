// <copyright file="WebDavServer.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using FubarDev.WebDavServer.Dispatchers;
using FubarDev.WebDavServer.Formatters;

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// The default WebDAV server implementation.
    /// </summary>
    public class WebDavServer : IWebDavDispatcher
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavServer"/> class.
        /// </summary>
        /// <param name="webDavClass1">The WebDAV class 1 implementation.</param>
        /// <param name="formatter">The formatter for the WebDAV XML responses.</param>
        /// <param name="webDavClass2">The WebDAV class 2 implementation.</param>
        public WebDavServer(IWebDavClass1 webDavClass1, IWebDavOutputFormatter formatter, IWebDavClass2? webDavClass2 = null)
        {
            Formatter = formatter;
            Class1 = webDavClass1;
            Class2 = webDavClass2;

            var supportedClasses = new List<IWebDavClass>() { webDavClass1 };
            if (webDavClass2 != null)
            {
                supportedClasses.Add(webDavClass2);
            }

            SupportedClasses = supportedClasses;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IWebDavClass> SupportedClasses { get; }

        /// <inheritdoc />
        public IWebDavOutputFormatter Formatter { get; }

        /// <inheritdoc />
        public IWebDavClass1 Class1 { get; }

        /// <inheritdoc />
        public IWebDavClass2? Class2 { get; }
    }
}
