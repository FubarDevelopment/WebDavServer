// <copyright file="WebDavHostOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.AspNetCore
{
    /// <summary>
    /// Options for the WebDAV host
    /// </summary>
    public class WebDavHostOptions
    {
        /// <summary>
        /// Gets or sets the base URL of the WebDAV server
        /// </summary>
        /// <remarks>
        /// This is usually required when run behind a proxy server.
        /// </remarks>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether anonymous WebDAV access is allowed.
        /// </summary>
        public bool AllowAnonymousAccess { get; set; }

        /// <summary>
        /// Gets or sets the home path for the unauthenticated user.
        /// </summary>
        public string AnonymousHomePath { get; set; }
    }
}
