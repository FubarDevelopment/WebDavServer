// <copyright file="EntryExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Dead;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Tests.Support
{
    public static class EntryExtensions
    {
        public static Task<IReadOnlyCollection<XElement>> GetPropertyElementsAsync(
            [NotNull] this IEntry entry,
            [NotNull] IWebDavDispatcher dispatcher,
            CancellationToken ct)
        {
            return GetPropertyElementsAsync(entry, dispatcher, false, ct);
        }

        public static async Task<IReadOnlyCollection<XElement>> GetPropertyElementsAsync(
            [NotNull] this IEntry entry,
            [NotNull] IWebDavDispatcher dispatcher,
            bool skipEtag,
            CancellationToken ct)
        {
            var result = new List<XElement>();
            using (var propEnum = entry.GetProperties(dispatcher).GetEnumerator())
            {
                while (await propEnum.MoveNext(ct).ConfigureAwait(false))
                {
                    var prop = propEnum.Current;
                    if (skipEtag && prop.Name == GetETagProperty.PropertyName)
                        continue;
                    var element = await prop.GetXmlValueAsync(ct).ConfigureAwait(false);
                    result.Add(element);
                }
            }

            return result;
        }
    }
}
