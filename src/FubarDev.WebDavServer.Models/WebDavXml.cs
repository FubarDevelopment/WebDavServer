// <copyright file="WebDavXml.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Globalization;
using System.Xml.Linq;

namespace FubarDev.WebDavServer
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

        /// <summary>
        /// Parses a timestamp in the RFC 1123 format.
        /// </summary>
        /// <param name="s">The timestamp to parse.</param>
        /// <returns>The parsed timestamp.</returns>
        internal static DateTimeOffset ParseRfc1123(string s)
        {
            // Workaround for invalid data from some clients.
            if (s.EndsWith("UTC"))
            {
                s = s[..^3] + "GMT";
            }

            return DateTimeOffset.ParseExact(s, "R", CultureInfo.InvariantCulture);
        }
    }
}
