// <copyright file="IfNoneMatchHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using FubarDev.WebDavServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model.Headers
{
    public class IfNoneMatchHeader : IIfHttpMatcher
    {
        [CanBeNull]
        private readonly ISet<EntityTag> _etags;

        private IfNoneMatchHeader([NotNull] IEnumerable<EntityTag> etags)
        {
            _etags = new HashSet<EntityTag>(etags, EntityTagComparer.Default);
        }

        private IfNoneMatchHeader()
        {
            _etags = null;
        }

        [NotNull]
        public static IfNoneMatchHeader Parse([CanBeNull] string s)
        {
            if (string.IsNullOrWhiteSpace(s) || s == "*")
                return new IfNoneMatchHeader();

            return new IfNoneMatchHeader(EntityTag.Parse(s));
        }

        [NotNull]
        public static IfNoneMatchHeader Parse([NotNull][ItemNotNull] IEnumerable<string> s)
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

            return new IfNoneMatchHeader(result);
        }

        public bool IsMatch(IEntry entry, EntityTag etag)
        {
            if (_etags == null)
                return false;
            return !_etags.Contains(etag);
        }
    }
}
