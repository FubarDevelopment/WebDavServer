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
    /// This is the equivalent of ASP.NET Cores <c>HttpContext</c>
    /// </remarks>
    public interface IWebDavContext
    {
        /// <summary>
        /// Gets the request protocol (<c>http</c> or <c>https</c>)
        /// </summary>
        [NotNull]
        string RequestProtocol { get; }

        /// <summary>
        /// Gets the relative request URL (e.g. <c>/webdav/path-to-controller/test.txt</c>)
        /// </summary>
        [NotNull]
        Uri PublicRelativeRequestUrl { get; }

        /// <summary>
        /// Gets the absolute request URL (e.g. <c>http://your-webdav-server/webdav/path-to-controller/test.txt</c>)
        /// </summary>
        [NotNull]
        Uri PublicAbsoluteRequestUrl { get; }

        /// <summary>
        /// Gets the base URL of the web service up to the controller path (e.g. <c>http://your-webdav-server/webdav/path-to-controller/</c>)
        /// </summary>
        [NotNull]
        Uri PublicControllerUrl { get; }

        /// <summary>
        /// Gets the base URL of the web service (e.g. <c>http://your-webdav-server/webdav/</c>)
        /// </summary>
        [NotNull]
        Uri PublicBaseUrl { get; }

        /// <summary>
        /// Gets the root URL of the web service (e.g. <c>http://your-webdav-server/</c>)
        /// </summary>
        [NotNull]
        Uri PublicRootUrl { get; }

        /// <summary>
        /// Gets the relative request URL (e.g. <c>/base-path/path-to-controller/test.txt</c>)
        /// </summary>
        [NotNull]
        Uri ServiceRelativeRequestUrl { get; }

        /// <summary>
        /// Gets the absolute request URL (e.g. <c>http://localhost/base-path/path-to-controller/test.txt</c>)
        /// </summary>
        [NotNull]
        Uri ServiceAbsoluteRequestUrl { get; }

        /// <summary>
        /// Gets the base URL of the web service (e.g. <c>http://localhost/base-path/</c>)
        /// </summary>
        [NotNull]
        Uri ServiceBaseUrl { get; }

        /// <summary>
        /// Gets the root URL of the web service (e.g. <c>http://localhost/</c>)
        /// </summary>
        [NotNull]
        Uri ServiceRootUrl { get; }

        /// <summary>
        /// Gets the relative request URL (e.g. <c>path-to-controller/</c>)
        /// </summary>
        [NotNull]
        Uri ControllerRelativeUrl { get; }

        /// <summary>
        /// Gets the relative request URL (e.g. <c>test.txt</c>)
        /// </summary>
        [NotNull]
        Uri ActionUrl { get; }

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

        /// <summary>
        /// Gets the WebDAV dispatcher
        /// </summary>
        [NotNull]
        IWebDavDispatcher Dispatcher { get; }
    }
}
