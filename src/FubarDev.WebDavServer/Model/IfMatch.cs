// <copyright file="IfMatch.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model
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

        public bool IsMatch(EntityTag etag, IReadOnlyCollection<Uri> stateTokens)
        {
            if (_etags == null)
                return true;
            return _etags.Contains(etag);
        }
    }
}
