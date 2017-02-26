// <copyright file="IWebDavClass.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Dispatchers
{
    /// <summary>
    /// Base interface for all WebDAV class implementations.
    /// </summary>
    public interface IWebDavClass
    {
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

        /// <summary>
        /// Gets the headers to be sent for a response of an <code>OPTIONS</code> request
        /// </summary>
        [NotNull]
        IReadOnlyDictionary<string, IEnumerable<string>> OptionsResponseHeaders { get; }

        /// <summary>
        /// Gets the headers to be sent for any response
        /// </summary>
        [NotNull]
        IReadOnlyDictionary<string, IEnumerable<string>> DefaultResponseHeaders { get; }

        /// <summary>
        /// Gets the properties for an entry that are supported by this WebDAV class
        /// </summary>
        /// <param name="entry">The entry to create the properties for</param>
        /// <returns>The properties that are to be used for the given <paramref name="entry"/></returns>
        [NotNull]
        [ItemNotNull]
        IEnumerable<IUntypedReadableProperty> GetProperties([NotNull] IEntry entry);
    }
}
