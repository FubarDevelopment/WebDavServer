// <copyright file="IDefaultDeadPropertyFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Store;

namespace FubarDev.WebDavServer.Props.Dead
{
    /// <summary>
    /// Factory for well-known (default) dead properties
    /// </summary>
    public interface IDefaultDeadPropertyFactory
    {
        /// <summary>
        /// Tries to create a well known dead property required/used by this WebDAV class implementation.
        /// </summary>
        /// <param name="store">The property store to store this property.</param>
        /// <param name="entry">The entry to instantiate this property for.</param>
        /// <param name="name">The name of the dead property to create.</param>
        /// <param name="deadProperty">The created dead property if this function returned <see langword="true"/>.</param>
        /// <returns><see langword="true"/> when this function could handle the creation of the well known dead property with the given <paramref name="name"/>.</returns>
        bool TryCreateDeadProperty(
            IPropertyStore store,
            IEntry entry,
            XName name,
            [NotNullWhen(true)] out IDeadProperty? deadProperty);

        /// <summary>
        /// Gets the properties for an entry that are supported by this WebDAV class.
        /// </summary>
        /// <param name="entry">The entry to create the properties for.</param>
        /// <returns>The properties that are to be used for the given <paramref name="entry"/>.</returns>
        IEnumerable<IUntypedReadableProperty> GetProperties(IEntry entry);
    }
}
