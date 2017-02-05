// <copyright file="If.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model
{
    public class If : IIfMatcher
    {
        private If([NotNull] [ItemNotNull] IReadOnlyCollection<IfList> lists)
        {
            Lists = lists;
        }

        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<IfList> Lists { get; }

        [NotNull]
        public static If Parse([NotNull] string s)
        {
            var source = new StringSource(s);
            var lists = IfList.Parse(source).ToList();
            if (source.Empty)
                throw new ArgumentException("Not an accepted list of conditions", nameof(s));
            return new If(lists);
        }

        public bool IsMatch(EntityTag etag, IReadOnlyCollection<Uri> stateTokens)
        {
            return Lists.Any(x => x.IsMatch(etag, stateTokens));
        }
    }
}
