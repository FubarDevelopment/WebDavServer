// <copyright file="IfNoneMatch.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model
{
    public class IfNoneMatch : IIfMatcher
    {
        [CanBeNull]
        private readonly ISet<EntityTag> _etags;

        private IfNoneMatch([NotNull] IEnumerable<EntityTag> etags)
        {
            _etags = new HashSet<EntityTag>(etags, EntityTagComparer.Default);
        }

        private IfNoneMatch()
        {
            _etags = null;
        }

        [NotNull]
        public static IfNoneMatch Parse([CanBeNull] string s)
        {
            if (string.IsNullOrWhiteSpace(s) || s == "*")
                return new IfNoneMatch();

            return new IfNoneMatch(EntityTag.Parse(s));
        }

        public bool IsMatch(EntityTag etag, IReadOnlyCollection<Uri> stateTokens)
        {
            if (_etags == null)
                return false;
            return !_etags.Contains(etag);
        }
    }
}
