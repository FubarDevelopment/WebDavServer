// <copyright file="WebDavXml.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

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
        public static XNamespace Dav { get; } = XNamespace.Get(WebDavNamespaceName);
    }
}
