// <copyright file="IWebDavContext.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Security.Principal;

using FubarDev.WebDavServer.Utils.UAParser;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// The WebDAV request context
    /// </summary>
    /// <remarks>
    /// This is the equivalent of ASP.NET Cores <code>HttpContext</code>
    /// </remarks>
    public interface IWebDavContext
    {
        /// <summary>
        /// Gets the request protocol (<code>http</code> or <code>https</code>)
        /// </summary>
        [NotNull]
        string RequestProtocol { get; }

        /// <summary>
        /// Gets the relative request URL (e.g. <code>/webdav/test.txt</code>)
        /// </summary>
        [NotNull]
        Uri RelativeRequestUrl { get; }

        /// <summary>
        /// Gets the absolute request URL (e.g. http://localhost/webdav/test.txt)
        /// </summary>
        [NotNull]
        Uri AbsoluteRequestUrl { get; }

        /// <summary>
        /// Gets the base URL of the web service (e.g. http://localhost/webdav/)
        /// </summary>
        [NotNull]
        Uri BaseUrl { get; }

        /// <summary>
        /// Gets the root URL of the web service (e.g. http://localhost/)
        /// </summary>
        [NotNull]
        Uri RootUrl { get; }

        /// <summary>
        /// Gets the parsed user agent
        /// </summary>
        [NotNull]
        IUAParserOutput DetectedClient { get; }

        /// <summary>
        /// Gets the request headers (partially parsed)
        /// </summary>
        [NotNull]
        IWebDavRequestHeaders RequestHeaders { get; }

        /// <summary>
        /// Gets the authenticated user
        /// </summary>
        [NotNull]
        IPrincipal User { get; }
    }
}
