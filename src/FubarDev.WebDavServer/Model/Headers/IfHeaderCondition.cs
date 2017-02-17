// <copyright file="IfHeaderCondition.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Utils;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model.Headers
{
    public class IfHeaderCondition : IIfMatcher
    {
        private IfHeaderCondition(bool not, Uri stateToken, EntityTag? etag)
        {
            Not = not;
            StateToken = stateToken;
            ETag = etag;
        }

        public bool Not { get; }

        [CanBeNull]
        public Uri StateToken { get; }

        public EntityTag? ETag { get; }

        public bool IsMatch(IEntry entry, EntityTag etag, IReadOnlyCollection<Uri> stateTokens)
        {
            bool result;

            if (ETag.HasValue)
            {
                result = etag == ETag.Value;
            }
            else
            {
                Debug.Assert(StateToken != null, "StateToken != null");
                result = stateTokens.Any(x => x.Equals(StateToken));
            }

            return Not ? !result : result;
        }

        [NotNull]
        [ItemNotNull]
        internal static IEnumerable<IfHeaderCondition> Parse([NotNull] StringSource source)
        {
            while (!source.SkipWhiteSpace())
            {
                var isNot = false;
                EntityTag? etag = null;
                if (source.AdvanceIf("Not", StringComparison.OrdinalIgnoreCase))
                {
                    isNot = true;
                    source.SkipWhiteSpace();
                }

                Uri stateToken;
                if (CodedUrlParser.TryParse(source, out stateToken))
                {
                    // Coded-URL found
                }
                else if (source.Get() == '[')
                {
                    // Entity-tag found
                    etag = EntityTag.Parse(source).Single();
                    if (!source.AdvanceIf("]"))
                        throw new ArgumentException($"{source.Remaining} is not a valid condition (ETag not ending with ']')", nameof(source));
                }
                else
                {
                    source.Back();
                    break;
                }

                yield return new IfHeaderCondition(isNot, stateToken, etag);
            }
        }
    }
}
