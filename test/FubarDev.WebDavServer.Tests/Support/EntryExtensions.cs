// <copyright file="EntryExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Dead;

namespace FubarDev.WebDavServer.Tests.Support
{
    public static class EntryExtensions
    {
        public static Task<IReadOnlyCollection<XElement>> GetPropertyElementsAsync(
            this IEntry entry,
            IDeadPropertyFactory deadPropertyFactory,
            CancellationToken ct)
        {
            return GetPropertyElementsAsync(entry, deadPropertyFactory, false, ct);
        }

        public static async Task<IReadOnlyCollection<XElement>> GetPropertyElementsAsync(
            this IEntry entry,
            IDeadPropertyFactory deadPropertyFactory,
            bool skipEtag,
            CancellationToken ct)
        {
            var result = await entry.GetProperties(deadPropertyFactory)
                .Where(x => !skipEtag || x.Name != GetETagProperty.PropertyName)
                .SelectAwait(async x => await x.GetXmlValueAsync(ct).ConfigureAwait(false))
                .ToListAsync(ct)
                .ConfigureAwait(false);
            return result;
        }
    }
}
