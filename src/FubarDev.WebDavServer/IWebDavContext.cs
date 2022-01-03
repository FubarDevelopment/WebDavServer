// <copyright file="IWebDavContext.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Security.Principal;

using FubarDev.WebDavServer.Utils.UAParser;

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
        /// Gets the request services.
        /// </summary>
        IServiceProvider RequestServices { get; }

        /// <summary>
        /// Gets the request protocol (<c>http</c> or <c>https</c>).
        /// </summary>
        string RequestProtocol { get; }

        /// <summary>
        /// Gets the HTTP request method (<c>GET</c>, etc...).
        /// </summary>
        string RequestMethod { get; }

        /// <summary>
        /// Gets the URL to be used as HREF.
        /// </summary>
        Uri HrefUrl { get; }

        /// <summary>
        /// Gets the request URL (e.g. <c>/webdav/path-to-controller/test.txt</c>) relative to <see cref="PublicRootUrl"/>.
        /// </summary>
        Uri PublicRelativeRequestUrl { get; }

        /// <summary>
        /// Gets the absolute request URL (e.g. <c>http://your-webdav-server/webdav/path-to-controller/test.txt</c>).
        /// </summary>
        Uri PublicAbsoluteRequestUrl { get; }

        /// <summary>
        /// Gets the base URL of the web service up to the controller path (e.g. <c>http://your-webdav-server/webdav/path-to-controller/</c>).
        /// </summary>
        Uri PublicControllerUrl { get; }

        /// <summary>
        /// Gets the base URL of the web service (e.g. <c>http://your-webdav-server/webdav/</c>).
        /// </summary>
        Uri PublicBaseUrl { get; }

        /// <summary>
        /// Gets the root URL of the web service (e.g. <c>http://your-webdav-server/</c>).
        /// </summary>
        Uri PublicRootUrl { get; }

        /// <summary>
        /// Gets the relative request URL (e.g. <c>/base-path/path-to-controller/test.txt</c>).
        /// </summary>
        Uri ServiceRelativeRequestUrl { get; }

        /// <summary>
        /// Gets the absolute request URL (e.g. <c>http://localhost/base-path/path-to-controller/test.txt</c>).
        /// </summary>
        Uri ServiceAbsoluteRequestUrl { get; }

        /// <summary>
        /// Gets the base URL of the web service (e.g. <c>http://localhost/base-path/</c>).
        /// </summary>
        Uri ServiceBaseUrl { get; }

        /// <summary>
        /// Gets the root URL of the web service (e.g. <c>http://localhost/</c>).
        /// </summary>
        Uri ServiceRootUrl { get; }

        /// <summary>
        /// Gets the relative request URL (e.g. <c>path-to-controller/</c>).
        /// </summary>
        Uri ControllerRelativeUrl { get; }

        /// <summary>
        /// Gets the relative request URL (e.g. <c>test.txt</c>).
        /// </summary>
        Uri ActionUrl { get; }

        /// <summary>
        /// Gets the parsed user agent.
        /// </summary>
        IUAParserOutput DetectedClient { get; }

        /// <summary>
        /// Gets the request headers (partially parsed).
        /// </summary>
        IWebDavRequestHeaders RequestHeaders { get; }

        /// <summary>
        /// Gets the authenticated user.
        /// </summary>
        IPrincipal User { get; }

        /// <summary>
        /// Gets the WebDAV dispatcher.
        /// </summary>
        IWebDavDispatcher Dispatcher { get; }
    }
}
