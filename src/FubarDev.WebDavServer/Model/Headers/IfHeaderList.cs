// <copyright file="IfHeaderList.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Properties;
using FubarDev.WebDavServer.Utils;

namespace FubarDev.WebDavServer.Model.Headers
{
    /// <summary>
    /// Represents a list of <see cref="IfHeaderCondition"/>s.
    /// </summary>
    public class IfHeaderList
    {
        private IfHeaderList(
            Uri resourceTag,
            Uri relativeHref,
            Uri path,
            IReadOnlyCollection<IfHeaderCondition> conditions)
        {
            ResourceTag = resourceTag;
            RelativeHref = relativeHref;
            Path = path;
            Conditions = conditions;
        }

        /// <summary>
        /// Gets the resource tag which is always an absolute URI.
        /// </summary>
        /// <remarks>
        /// When a relative URI gets sent from the client, then it gets converted into an
        /// absolute URI.
        /// </remarks>
        public Uri ResourceTag { get; }

        /// <summary>
        /// Gets the resource tag relative to the <see cref="IWebDavContext.PublicRootUrl"/>.
        /// </summary>
        /// <remarks>
        /// Might be an absolute URL when the host or scheme don't match.
        /// </remarks>
        public Uri RelativeHref { get; }

        /// <summary>
        /// Gets the path to the destination relative to the <see cref="IWebDavContext.PublicBaseUrl"/>.
        /// </summary>
        /// <remarks>
        /// Might be an absolute URL when the host or scheme don't match.
        /// </remarks>
        public Uri Path { get; }

        /// <summary>
        /// Gets the collection of conditions that must be satisfied by this list.
        /// </summary>
        public IReadOnlyCollection<IfHeaderCondition> Conditions { get; }

        /// <summary>
        /// Gets a value indicating whether this condition list requires the <see cref="EntityTag"/> of an <see cref="IEntry"/> for
        /// the evaluation.
        /// </summary>
        public bool RequiresEntityTag => Conditions.Any(x => x.ETag != null && !x.Not);

        /// <summary>
        /// Gets a value indicating whether this condition list requires the <see cref="IActiveLock.StateToken"/> for the evaluation.
        /// </summary>
        public bool RequiresStateToken => Conditions.Any(x => x.StateToken != null && !x.Not);

        /// <summary>
        /// Validates if all conditions match the passed entity tag and/or state tokens.
        /// </summary>
        /// <param name="etag">The entity tag.</param>
        /// <param name="stateTokens">The state tokens.</param>
        /// <returns><see langword="true"/> when this condition matches.</returns>
        public bool IsMatch(
            EntityTag? etag,
            IReadOnlyCollection<Uri> stateTokens)
        {
            return Conditions.All(x => x.IsMatch(etag, stateTokens));
        }

        internal static IEnumerable<IfHeaderList> Parse(
            StringSource source,
            EntityTagComparer etagComparer,
            IWebDavContext context)
        {
            Uri previousResourceTag = context.PublicAbsoluteRequestUrl;
            while (!source.SkipWhiteSpace())
            {
                if (CodedUrlParser.TryParse(source, out var resourceTag))
                {
                    // Coded-URL found
                    if (!resourceTag.IsAbsoluteUri)
                    {
                        resourceTag = new Uri(context.PublicRootUrl, resourceTag);
                    }

                    previousResourceTag = resourceTag;
                    source.SkipWhiteSpace();
                }
                else
                {
                    resourceTag = previousResourceTag;
                }

                if (!source.AdvanceIf("("))
                {
                    throw new ArgumentException(
                        string.Format(Resources.ListNotStartingWithBracket, source.Remaining),
                        nameof(source));
                }

                var conditions = IfHeaderCondition.Parse(source, etagComparer).ToList();
                if (!source.AdvanceIf(")"))
                {
                    throw new ArgumentException(
                        string.Format(Resources.ListNotEndingWithBracket, source.Remaining),
                        nameof(source));
                }

                var relativeHref = context.PublicControllerUrl.IsBaseOf(resourceTag) ? AddRootSlashToUri(context.PublicControllerUrl.MakeRelativeUri(resourceTag)) : resourceTag;
                var path = context.PublicControllerUrl.IsBaseOf(resourceTag) ? context.PublicControllerUrl.MakeRelativeUri(resourceTag) : resourceTag;
                yield return new IfHeaderList(resourceTag, relativeHref, path, conditions);
            }
        }

        private static Uri AddRootSlashToUri(Uri url)
        {
            if (url.IsAbsoluteUri)
            {
                return url;
            }

            var s = url.OriginalString;
            if (s.StartsWith("/"))
            {
                return url;
            }

            return new Uri("/" + s, UriKind.Relative);
        }
    }
}
