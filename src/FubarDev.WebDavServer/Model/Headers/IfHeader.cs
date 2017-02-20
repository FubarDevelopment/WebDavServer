// <copyright file="IfHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Utils;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model.Headers
{
    public class IfHeader
    {
        private IfHeader([NotNull] [ItemNotNull] IReadOnlyCollection<IfHeaderList> lists)
        {
            Lists = lists;
        }

        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<IfHeaderList> Lists { get; }

        [NotNull]
        public static IfHeader Parse([NotNull] string s, [NotNull] EntityTagComparer etagComparer, [CanBeNull] Uri requestUrl)
        {
            var source = new StringSource(s);
            var lists = IfHeaderList.Parse(source, etagComparer, requestUrl).ToList();
            if (source.Empty)
                throw new ArgumentException("Not an accepted list of conditions", nameof(s));
            return new IfHeader(lists);
        }

        public bool IsMatch(EntityTag? etag, IReadOnlyCollection<Uri> stateTokens)
        {
            return Lists.Any(x => x.IsMatch(etag, stateTokens));
        }

        public async Task<bool> IsMatchAsync(IEntry entry, IReadOnlyCollection<Uri> stateTokens, CancellationToken cancellationToken)
        {
            var etag = await entry.GetEntityTagAsync(cancellationToken).ConfigureAwait(false);
            return IsMatch(etag, stateTokens);
        }
    }
}
