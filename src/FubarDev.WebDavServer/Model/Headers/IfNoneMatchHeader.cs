// <copyright file="IfNoneMatchHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model.Headers
{
    public class IfNoneMatchHeader
    {
        [CanBeNull]
        private readonly ISet<EntityTag> _etags;

        private IfNoneMatchHeader([NotNull] IEnumerable<EntityTag> etags, [NotNull] EntityTagComparer etagComparer)
        {
            _etags = new HashSet<EntityTag>(etags, etagComparer);
        }

        private IfNoneMatchHeader()
        {
            _etags = null;
        }

        [NotNull]
        public static IfNoneMatchHeader Parse([CanBeNull] string s)
        {
            return Parse(s, EntityTagComparer.Strong);
        }

        [NotNull]
        public static IfNoneMatchHeader Parse([CanBeNull] string s, [NotNull] EntityTagComparer etagComparer)
        {
            if (string.IsNullOrWhiteSpace(s) || s == "*")
                return new IfNoneMatchHeader();

            return new IfNoneMatchHeader(EntityTag.Parse(s), etagComparer);
        }

        [NotNull]
        public static IfNoneMatchHeader Parse([NotNull] [ItemNotNull] IEnumerable<string> s)
        {
            return Parse(s, EntityTagComparer.Strong);
        }

        [NotNull]
        public static IfNoneMatchHeader Parse([NotNull][ItemNotNull] IEnumerable<string> s, [NotNull] EntityTagComparer etagComparer)
        {
            var result = new List<EntityTag>();
            foreach (var etag in s)
            {
                if (etag == "*")
                    return new IfNoneMatchHeader();

                result.AddRange(EntityTag.Parse(etag));
            }

            if (result.Count == 0)
                return new IfNoneMatchHeader();

            return new IfNoneMatchHeader(result, etagComparer);
        }

        public bool IsMatch(EntityTag? etag)
        {
            if (_etags == null)
                return false;
            if (etag == null)
                return true;
            return !_etags.Contains(etag.Value);
        }

        public async Task<bool> IsMatchAsync(IEntry entry, CancellationToken cancellationToken)
        {
            if (_etags == null)
                return false;
            var etag = await entry.GetEntityTagAsync(cancellationToken).ConfigureAwait(false);
            return IsMatch(etag);
        }
    }
}
