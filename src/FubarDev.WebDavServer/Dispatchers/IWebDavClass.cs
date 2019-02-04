// <copyright file="IWebDavClass.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Store;

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
        /// Gets the context for a request.
        /// </summary>
        [NotNull]
        IWebDavContext WebDavContext { get; }

        /// <summary>
        /// Gets the headers to be sent for a response of an <c>OPTIONS</c> request.
        /// </summary>
        [NotNull]
        IReadOnlyDictionary<string, IEnumerable<string>> OptionsResponseHeaders { get; }

        /// <summary>
        /// Gets the headers to be sent for any response.
        /// </summary>
        [NotNull]
        IReadOnlyDictionary<string, IEnumerable<string>> DefaultResponseHeaders { get; }

        /// <summary>
        /// Gets the properties for an entry that are supported by this WebDAV class.
        /// </summary>
        /// <param name="entry">The entry to create the properties for.</param>
        /// <returns>The properties that are to be used for the given <paramref name="entry"/>.</returns>
        [NotNull]
        [ItemNotNull]
        IEnumerable<IUntypedReadableProperty> GetProperties([NotNull] IEntry entry);

        /// <summary>
        /// Tries to create a well known dead property required/used by this WebDAV class implementation.
        /// </summary>
        /// <param name="store">The property store to store this property.</param>
        /// <param name="entry">The entry to instantiate this property for.</param>
        /// <param name="name">The name of the dead property to create.</param>
        /// <param name="deadProperty">The created dead property if this function returned <see langword="true"/>.</param>
        /// <returns><see langword="true"/> when this function could handle the creation of the well known dead property with the given <paramref name="name"/>.</returns>
        bool TryCreateDeadProperty([NotNull] IPropertyStore store, [NotNull] IEntry entry, [NotNull] XName name, out IDeadProperty deadProperty);
    }
}
