// <copyright file="IfMatchHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model.Headers
{
    public class IfMatchHeader
    {
        [CanBeNull]
        private readonly ISet<EntityTag> _etags;

        public IfMatchHeader()
        {
            _etags = null;
        }

        public IfMatchHeader([NotNull] IEnumerable<EntityTag> etags)
            : this(etags, EntityTagComparer.Strong)
        {
        }

        public IfMatchHeader([NotNull] IEnumerable<EntityTag> etags, EntityTagComparer etagComparer)
        {
            _etags = new HashSet<EntityTag>(etags, etagComparer);
        }

        [NotNull]
        public static IfMatchHeader Parse([CanBeNull] string s)
        {
            return Parse(s, EntityTagComparer.Strong);
        }

        [NotNull]
        public static IfMatchHeader Parse([CanBeNull] string s, EntityTagComparer etagComparer)
        {
            if (string.IsNullOrWhiteSpace(s) || s == "*")
                return new IfMatchHeader();

            return new IfMatchHeader(EntityTag.Parse(s), etagComparer);
        }

        [NotNull]
        public static IfMatchHeader Parse([NotNull] [ItemNotNull] IEnumerable<string> s)
        {
            return Parse(s, EntityTagComparer.Strong);
        }

        [NotNull]
        public static IfMatchHeader Parse([NotNull][ItemNotNull] IEnumerable<string> s, EntityTagComparer etagComparer)
        {
            var result = new List<EntityTag>();
            foreach (var etag in s)
            {
                if (etag == "*")
                    return new IfMatchHeader();

                result.AddRange(EntityTag.Parse(etag));
            }

            if (result.Count == 0)
                return new IfMatchHeader();

            return new IfMatchHeader(result, etagComparer);
        }

        public bool IsMatch(EntityTag? etag)
        {
            if (_etags == null)
                return true;
            if (etag == null)
                return false;
            return _etags.Contains(etag.Value);
        }

        public async Task<bool> IsMatchAsync(IEntry entry, CancellationToken cancellationToken)
        {
            if (_etags == null)
                return true;
            var etag = await entry.GetEntityTagAsync(cancellationToken).ConfigureAwait(false);
            return IsMatch(etag);
        }
    }
}
