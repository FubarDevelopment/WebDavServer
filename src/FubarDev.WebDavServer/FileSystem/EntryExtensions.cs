// <copyright file="EntryExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model.Headers;

namespace FubarDev.WebDavServer.FileSystem
{
    public static class EntryExtensions
    {
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
