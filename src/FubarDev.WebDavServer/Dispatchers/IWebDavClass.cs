// <copyright file="IWebDavClass.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Dispatchers
{
    /// <summary>
    /// Base interface for all WebDAV class implementations.
    /// </summary>
    public interface IWebDavClass
    {
        /// <summary>
        /// Gets the version of the WebDAV class
        /// </summary>
        [NotNull]
        string Version { get; }

        /// <summary>
        /// Gets the HTTP methods supported by this WebDAV class.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        IEnumerable<string> HttpMethods { get; }

        /// <summary>
        /// Gets the context for a request
        /// </summary>
        [NotNull]
        IWebDavContext WebDavContext { get; }
    }
}
