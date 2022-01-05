// <copyright file="IfHeaderCondition.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using FubarDev.WebDavServer.Models;
using FubarDev.WebDavServer.Properties;
using FubarDev.WebDavServer.Utils;

namespace FubarDev.WebDavServer.Model.Headers
{
    /// <summary>
    /// Represents a single condition for an HTTP <c>If</c> header.
    /// </summary>
    [Obsolete]
    public class IfHeaderCondition
    {
        private readonly EntityTagComparer _etagComparer;

        private IfHeaderCondition(bool not, Uri? stateToken, EntityTag? etag, EntityTagComparer etagComparer)
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
        public Uri? StateToken { get; }

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
        [Obsolete]
        public bool IsMatch(
            EntityTag? etag,
            IReadOnlyCollection<Uri> stateTokens)
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

        internal static IEnumerable<IfHeaderCondition> Parse(StringSource source, EntityTagComparer entityTagComparer)
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

                if (CodedUrlParser.TryParse(source, out var stateToken))
                {
                    // Coded-URL found
                }
                else if (source.Get() == '[')
                {
                    // Entity-tag found
                    etag = ParseEntityTag(source).Single();
                    if (!source.AdvanceIf("]"))
                    {
                        throw new ArgumentException(
                            string.Format(Resources.ETagNotEndingWithBracket, source.Remaining),
                            nameof(source));
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

        private static IEnumerable<EntityTag> ParseEntityTag(StringSource source)
        {
            while (!source.SkipWhiteSpace())
            {
                bool isWeak;
                if (source.AdvanceIf("W/\"", StringComparison.OrdinalIgnoreCase))
                {
                    isWeak = true;
                }
                else if (!source.AdvanceIf("\""))
                {
                    break;
                }
                else
                {
                    isWeak = false;
                }

                var etagText = source.GetUntil('"');
                if (etagText == null)
                {
                    throw new ArgumentException($@"{source.Remaining} is not a valid ETag", nameof(source));
                }

                yield return new EntityTag(isWeak, etagText);

                if (source.Advance(1).SkipWhiteSpace())
                {
                    break;
                }

                if (!source.AdvanceIf(","))
                {
                    break;
                }
            }
        }
    }
}
