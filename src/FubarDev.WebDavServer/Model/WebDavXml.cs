// <copyright file="WebDavXml.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model
{
    /// <summary>
    /// Utility properties for the WebDAV XML.
    /// </summary>
    public static class WebDavXml
    {
        private const string WebDavNamespaceName = "DAV:";

        /// <summary>
        /// Gets the WebDAV namespace.
        /// </summary>
        [NotNull]
        public static XNamespace Dav { get; } = XNamespace.Get(WebDavNamespaceName);
    }
}
