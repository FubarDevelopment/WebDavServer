// <copyright file="EntryExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.Props;

namespace FubarDev.WebDavServer.FileSystem
{
    /// <summary>
    /// Extension methods for <see cref="IEntry"/> implementations
    /// </summary>
    public static class EntryExtensions
    {
        /// <summary>
        /// Gets the <see cref="EntityTag"/> for the <paramref name="entry"/>
        /// </summary>
        /// <remarks>
        /// The return value might be null, when no property store was defined.
        /// </remarks>
        /// <param name="entry">The entry to get the <see cref="EntityTag"/> for</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The entity tag for the entry</returns>
        public static async Task<EntityTag?> GetEntityTagAsync(this IEntry entry, CancellationToken cancellationToken)
        {
            var etagEntry = entry as IEntityTagEntry;
            if (etagEntry != null)
                return etagEntry.ETag;
            var propStore = entry.FileSystem.PropertyStore;
            if (propStore == null)
                return null;
            return await propStore.GetETagAsync(entry, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets all predefined properties for the given <paramref name="entry"/>, provided by the given <paramref name="dispatcher"/>.
        /// </summary>
        /// <param name="entry">The entry to get the properties for</param>
        /// <param name="dispatcher">The dispatcher that provides the predefined properties</param>
        /// <param name="maxCost">The maximum cost for querying a properties value</param>
        /// <param name="returnInvalidProperties">Do we want to get invalid live properties?</param>
        /// <returns>The async enumerable of all property (including the property store when the <paramref name="maxCost"/> allows it)</returns>
        public static IAsyncEnumerable<IUntypedReadableProperty> GetProperties(this IEntry entry, IWebDavDispatcher dispatcher, int? maxCost = null, bool returnInvalidProperties = false)
        {
            var properties = new List<IUntypedReadableProperty>();
            foreach (var webDavClass in dispatcher.SupportedClasses)
            {
                properties.AddRange(webDavClass.GetProperties(entry));
            }

            return new EntryProperties(entry, properties, entry.FileSystem.PropertyStore, maxCost, returnInvalidProperties);
        }
    }
}
