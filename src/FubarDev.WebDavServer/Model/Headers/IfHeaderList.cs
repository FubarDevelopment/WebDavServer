// <copyright file="IfHeaderList.cs" company="Fubar Development Junker">
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
    public class IfHeaderList : IIfWebDavMatcher
    {
        private IfHeaderList(
            [CanBeNull] Uri resourceTag,
            [NotNull] [ItemNotNull] IReadOnlyCollection<IfHeaderCondition> conditions)
        {
            ResourceTag = resourceTag;
            Conditions = conditions;
        }

        [CanBeNull]
        public Uri ResourceTag { get; }

        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<IfHeaderCondition> Conditions { get; }

        public bool IsMatch(IEntry entry, EntityTag etag, IReadOnlyCollection<Uri> stateTokens)
        {
            return Conditions.All(x => x.IsMatch(entry, etag, stateTokens));
        }

        [NotNull]
        [ItemNotNull]
        internal static IEnumerable<IfHeaderList> Parse([NotNull] StringSource source)
        {
            while (!source.SkipWhiteSpace())
            {
                Uri resourceTag;
                if (CodedUrlParser.TryParse(source, out resourceTag))
                {
                    // Coded-URL found
                    source.SkipWhiteSpace();
                }
                else
                {
                    resourceTag = null;
                }

                if (!source.AdvanceIf("("))
                    throw new ArgumentException($"{source.Remaining} is not a valid list (not starting with a '(')", nameof(source));
                var conditions = IfHeaderCondition.Parse(source).ToList();
                if (!source.AdvanceIf(")"))
                    throw new ArgumentException($"{source.Remaining} is not a valid list (not ending with a ')')", nameof(source));

                yield return new IfHeaderList(resourceTag, conditions);
            }
        }
    }
}
