// <copyright file="WebDavServer.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

using FubarDev.WebDavServer.Dispatchers;
using FubarDev.WebDavServer.Formatters;

using JetBrains.Annotations;

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
        public WebDavServer([NotNull] IWebDavClass1 webDavClass1, [NotNull] IWebDavOutputFormatter formatter, [CanBeNull] IWebDavClass2 webDavClass2 = null)
        {
            Formatter = formatter;
            Class1 = webDavClass1;
            Class2 = webDavClass2;
            SupportedClasses = new IWebDavClass[] { webDavClass1, webDavClass2 }.Where(x => x != null).ToList();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IWebDavClass> SupportedClasses { get; }

        /// <inheritdoc />
        public IWebDavOutputFormatter Formatter { get; }

        /// <inheritdoc />
        public IWebDavClass1 Class1 { get; }

        /// <inheritdoc />
        public IWebDavClass2 Class2 { get; }
    }
}
