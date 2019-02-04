// <copyright file="IfHeaderCondition.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using FubarDev.WebDavServer.Utils;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model.Headers
{
    /// <summary>
    /// Represents a single condition for an HTTP <c>If</c> header.
    /// </summary>
    public class IfHeaderCondition
    {
        [NotNull]
        private readonly EntityTagComparer _etagComparer;

        private IfHeaderCondition(bool not, [CanBeNull] Uri stateToken, EntityTag? etag, [NotNull] EntityTagComparer etagComparer)
        {
            _etagComparer = etagComparer;
            Not = not;
            StateToken = stateToken;
            ETag = etag;
        }

        /// <summary>
        /// Gets a value indicating whether the result should be negated.
        /// </summary>
        public bool Not { get; }

        /// <summary>
        /// Gets the state token to validate with.
        /// </summary>
        [CanBeNull]
        public Uri StateToken { get; }

        /// <summary>
        /// Gets the entity tag to validate with.
        /// </summary>
        public EntityTag? ETag { get; }

        /// <summary>
        /// Validates if this condition matches the passed entity tag and/or state tokens.
        /// </summary>
        /// <param name="etag">The entity tag.</param>
        /// <param name="stateTokens">The state tokens.</param>
        /// <returns><see langword="true"/> when this condition matches.</returns>
        public bool IsMatch(EntityTag? etag, IReadOnlyCollection<Uri> stateTokens)
        {
            bool result;

            if (ETag.HasValue)
            {
                if (etag == null)
                {
                    return false;
                }

                result = _etagComparer.Equals(etag.Value, ETag.Value);
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
        internal static IEnumerable<IfHeaderCondition> Parse([NotNull] StringSource source, [NotNull] EntityTagComparer entityTagComparer)
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
                    {
                        throw new ArgumentException($"{source.Remaining} is not a valid condition (ETag not ending with ']')", nameof(source));
                    }
                }
                else
                {
                    source.Back();
                    break;
                }

                yield return new IfHeaderCondition(isNot, stateToken, etag, entityTagComparer);
            }
        }
    }
}
