// <copyright file="IfMatchHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using FubarDev.WebDavServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model.Headers
{
    public class IfMatchHeader : IIfMatcher
    {
        [CanBeNull]
        private readonly ISet<EntityTag> _etags;

        private IfMatchHeader([NotNull] IEnumerable<EntityTag> etags)
        {
            _etags = new HashSet<EntityTag>(etags, EntityTagComparer.Default);
        }

        private IfMatchHeader()
        {
            _etags = null;
        }

        [NotNull]
        public static IfMatchHeader Parse([CanBeNull] string s)
        {
            if (string.IsNullOrWhiteSpace(s) || s == "*")
                return new IfMatchHeader();

            return new IfMatchHeader(EntityTag.Parse(s));
        }

        [NotNull]
        public static IfMatchHeader Parse([NotNull][ItemNotNull] IEnumerable<string> s)
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

            return new IfMatchHeader(result);
        }

        public bool IsMatch(IEntry entry, EntityTag etag, IReadOnlyCollection<Uri> stateTokens)
        {
            if (_etags == null)
                return true;
            return _etags.Contains(etag);
        }
    }
}
