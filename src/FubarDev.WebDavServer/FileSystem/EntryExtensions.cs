// <copyright file="EntryExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model.Headers;

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
    }
}
