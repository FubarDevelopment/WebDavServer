// <copyright file="WebDavServerOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// Options for the WebDAV service configuration.
    /// </summary>
    public class WebDavServerOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether Class 2 functionality should be enabled.
        /// </summary>
        public bool EnableClass2 { get; set; } = true;
    }
}
