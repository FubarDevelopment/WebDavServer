// <copyright file="IDeadPropertyFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Store;

namespace FubarDev.WebDavServer.Props.Dead
{
    /// <summary>
    /// The interface for a dead property factory
    /// </summary>
    public interface IDeadPropertyFactory
    {
        /// <summary>
        /// Creates a new dead property instance
        /// </summary>
        /// <param name="store">The property store to store this property</param>
        /// <param name="entry">The entry to instantiate this property for</param>
        /// <param name="name">The name of the dead property to create</param>
        /// <returns>The created dead property instance</returns>
        IDeadProperty Create(IPropertyStore store, IEntry entry, XName name);

        /// <summary>
        /// Creates a new dead property instance
        /// </summary>
        /// <param name="store">The property store to store this property</param>
        /// <param name="entry">The entry to instantiate this property for</param>
        /// <param name="element">The element to intialize the dead property with</param>
        /// <returns>The created dead property instance</returns>
        IDeadProperty Create(IPropertyStore store, IEntry entry, XElement element);
    }
}
