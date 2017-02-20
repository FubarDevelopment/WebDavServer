// <copyright file="IfHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Utils;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model.Headers
{
    public class IfHeader : IIfWebDavMatcher
    {
        private IfHeader([NotNull] [ItemNotNull] IReadOnlyCollection<IfHeaderList> lists)
        {
            Lists = lists;
        }

        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<IfHeaderList> Lists { get; }

        [NotNull]
        public static IfHeader Parse([NotNull] string s)
        {
            var source = new StringSource(s);
            var lists = IfHeaderList.Parse(source).ToList();
            if (source.Empty)
                throw new ArgumentException("Not an accepted list of conditions", nameof(s));
            return new IfHeader(lists);
        }

        public bool IsMatch(IEntry entry, EntityTag etag, IReadOnlyCollection<Uri> stateTokens)
        {
            return Lists.Any(x => x.IsMatch(entry, etag, stateTokens));
        }
    }
}
