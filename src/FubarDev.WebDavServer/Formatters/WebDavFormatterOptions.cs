// <copyright file="WebDavFormatterOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Formatters
{
    /// <summary>
    /// Options for the WebDAV XML output formatter.
    /// </summary>
    public class WebDavFormatterOptions
    {
        /// <summary>
        /// Gets or sets the content type to send.
        /// </summary>
        [CanBeNull]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the namespace prefix to use.
        /// </summary>
        [NotNull]
        public string NamespacePrefix { get; set; } = "D";
    }
}
