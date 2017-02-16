// <copyright file="IfMatch.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using FubarDev.WebDavServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model.Headers
{
    public class IfMatch : IIfMatcher
    {
        [CanBeNull]
        private readonly ISet<EntityTag> _etags;

        private IfMatch([NotNull] IEnumerable<EntityTag> etags)
        {
            _etags = new HashSet<EntityTag>(etags, EntityTagComparer.Default);
        }

        private IfMatch()
        {
            _etags = null;
        }

        [NotNull]
        public static IfMatch Parse([CanBeNull] string s)
        {
            if (string.IsNullOrWhiteSpace(s) || s == "*")
                return new IfMatch();

            return new IfMatch(EntityTag.Parse(s));
        }

        [NotNull]
        public static IfMatch Parse([NotNull][ItemNotNull] IEnumerable<string> s)
        {
            var result = new List<EntityTag>();
            foreach (var etag in s)
            {
                if (etag == "*")
                    return new IfMatch();

                result.AddRange(EntityTag.Parse(etag));
            }

            if (result.Count == 0)
                return new IfMatch();

            return new IfMatch(result);
        }

        public bool IsMatch(IEntry entry, EntityTag etag, IReadOnlyCollection<Uri> stateTokens)
        {
            if (_etags == null)
                return true;
            return _etags.Contains(etag);
        }
    }
}
