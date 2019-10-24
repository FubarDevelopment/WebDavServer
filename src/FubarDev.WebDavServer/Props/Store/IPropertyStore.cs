// <copyright file="IPropertyStore.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.Props.Dead;

namespace FubarDev.WebDavServer.Props.Store
{
    /// <summary>
    /// The interface a property store (for dead properties) must implement
    /// </summary>
    public interface IPropertyStore
    {
        /// <summary>
        /// Gets the cost to query the properties of a property store
        /// </summary>
        int Cost { get; }

        /// <summary>
        /// Gets a dead property with the given <paramref name="name"/> for the given <paramref name="entry"/>
        /// </summary>
        /// <param name="entry">The entry to get the property with the given <paramref name="name"/> for</param>
        /// <param name="name">The name of the parameter to get for a given <paramref name="entry"/></param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The <see cref="XElement"/> for the given dead property</returns>
        /// <remarks>A <see cref="GetETagProperty"/> will not be returned by this function.</remarks>
        Task<XElement?> GetAsync(IEntry entry, XName name, CancellationToken cancellationToken);

        /// <summary>
        /// Sets a dead property for the given <paramref name="entry"/> to the given <paramref name="element"/>
        /// </summary>
        /// <param name="entry">The entry to set the property <paramref name="element"/> for</param>
        /// <param name="element">The element to set the <paramref name="entry"/> for</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The async task</returns>
        /// <remarks>A <see cref="GetETagProperty"/> cannot be updated by this function.</remarks>
        Task SetAsync(IEntry entry, XElement element, CancellationToken cancellationToken);

        /// <summary>
        /// Removes a dead property with a given <paramref name="name"/> from the given <paramref name="entry"/>
        /// </summary>
        /// <param name="entry">The entry to remove the dead property with the given <paramref name="name"/> from</param>
        /// <param name="name">The name of the parameter to remove from the <paramref name="entry"/></param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns><see langword="true"/> when there was a dead property with the given <paramref name="name"/> that could be removed</returns>
        /// <remarks>A <see cref="GetETagProperty"/> cannot be removed by this function.</remarks>
        Task<bool> RemoveAsync(IEntry entry, XName name, CancellationToken cancellationToken);

        /// <summary>
        /// Gets all dead properties for a given <paramref name="entry"/>
        /// </summary>
        /// <param name="entry">The entry to get all the dead properties for</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The collection of all dead properties</returns>
        /// <remarks>A <see cref="GetETagProperty"/> will not be returned by this function.</remarks>
        Task<IReadOnlyCollection<XElement>> GetAsync(IEntry entry, CancellationToken cancellationToken);

        /// <summary>
        /// Sets all given dead <paramref name="properties"/> for the given <paramref name="entry"/>
        /// </summary>
        /// <param name="entry">The entry to set all the dead <paramref name="properties"/> for</param>
        /// <param name="properties">The properties to set for the given <paramref name="entry"/></param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The async task</returns>
        /// <remarks>A <see cref="GetETagProperty"/> cannot be updated using this method</remarks>
        Task SetAsync(IEntry entry, IEnumerable<XElement> properties, CancellationToken cancellationToken);

        /// <summary>
        /// Remove multiple dead properties by its name at once from the given <paramref name="entry"/>
        /// </summary>
        /// <param name="entry">The entry to remove the given property <paramref name="names"/> from</param>
        /// <param name="names">The names of the dead properties to remove from the given <paramref name="entry"/></param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of booleans where a <see langword="true"/> value indicates that there was a dead property for a given
        /// name that could be removed from the <paramref name="entry"/>. A <see cref="GetETagProperty"/> cannot be removed
        /// by this function.</returns>
        Task<IReadOnlyCollection<bool>> RemoveAsync(IEntry entry, IEnumerable<XName> names, CancellationToken cancellationToken);

        /// <summary>
        /// Remove all dead propertied (including a probably exting <see cref="GetETagProperty"/>) from a given <paramref name="entry"/>
        /// </summary>
        /// <param name="entry">The entry to remove all the dead properties from</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The async task</returns>
        Task RemoveAsync(IEntry entry, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a dead property by its <paramref name="name"/> for a given <paramref name="entry"/>
        /// </summary>
        /// <param name="entry">The entry to create the dead property with the given <paramref name="name"/> for</param>
        /// <param name="name">The name of the dead property to create for the given <paramref name="entry"/> for</param>
        /// <returns>The created dead property with the given <paramref name="name"/> for the given <paramref name="entry"/></returns>
        IDeadProperty Create(IEntry entry, XName name);

        /// <summary>
        /// Loads the dead property with the given <paramref name="name"/> into a <see cref="IDeadProperty"/> implementation
        /// </summary>
        /// <param name="entry">The entry to load the dead property for</param>
        /// <param name="name">The name of the dead property</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The implementation of the dead property</returns>
        Task<IDeadProperty> LoadAsync(IEntry entry, XName name, CancellationToken cancellationToken);

        /// <summary>
        /// Loads all dead properties for a given <paramref name="entry"/> into <see cref="IDeadProperty"/> implementations
        /// </summary>
        /// <param name="entry">The entry to load the dead properties for</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The collection of the loaded dead properties</returns>
        Task<IReadOnlyCollection<IDeadProperty>> LoadAsync(IEntry entry, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the entity tag for a given <paramref name="entry"/>
        /// </summary>
        /// <param name="entry">The entry to get the entity tag for</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The loaded entity tag</returns>
        Task<EntityTag> GetETagAsync(IEntry entry, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an entity tag for a given <paramref name="entry"/>
        /// </summary>
        /// <param name="entry">The entry to get the entity tag for</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The updated entity tag</returns>
        Task<EntityTag> UpdateETagAsync(IEntry entry, CancellationToken cancellationToken);
    }
}
