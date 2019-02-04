// <copyright file="IfNoneMatchHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model.Headers
{
    /// <summary>
    /// Class that represents the HTTP <c>If-None-Match</c> header.
    /// </summary>
    public class IfNoneMatchHeader
    {
        [CanBeNull]
        private readonly ISet<EntityTag> _etags;

        /// <summary>
        /// Initializes a new instance of the <see cref="IfNoneMatchHeader"/> class.
        /// </summary>
        /// <param name="etags">The entity tags to match.</param>
        /// <param name="etagComparer">The entity comparer to use.</param>
        private IfNoneMatchHeader([NotNull] IEnumerable<EntityTag> etags, [NotNull] EntityTagComparer etagComparer)
        {
            _etags = new HashSet<EntityTag>(etags, etagComparer);
        }

        private IfNoneMatchHeader()
        {
            _etags = null;
        }

        /// <summary>
        /// Parses the header string to get a new instance of the <see cref="IfNoneMatchHeader"/> class.
        /// </summary>
        /// <param name="s">The header string to parse.</param>
        /// <returns>The new instance of the <see cref="IfNoneMatchHeader"/> class.</returns>
        [NotNull]
        public static IfNoneMatchHeader Parse([CanBeNull] string s)
        {
            return Parse(s, EntityTagComparer.Strong);
        }

        /// <summary>
        /// Parses the header string to get a new instance of the <see cref="IfNoneMatchHeader"/> class.
        /// </summary>
        /// <param name="s">The header string to parse.</param>
        /// <param name="etagComparer">The entity tag comparer used for the <see cref="IsMatch"/> function.</param>
        /// <returns>The new instance of the <see cref="IfNoneMatchHeader"/> class.</returns>
        [NotNull]
        public static IfNoneMatchHeader Parse([CanBeNull] string s, [NotNull] EntityTagComparer etagComparer)
        {
            if (string.IsNullOrWhiteSpace(s) || s == "*")
            {
                return new IfNoneMatchHeader();
            }

            return new IfNoneMatchHeader(EntityTag.Parse(s), etagComparer);
        }

        /// <summary>
        /// Parses the header string to get a new instance of the <see cref="IfNoneMatchHeader"/> class.
        /// </summary>
        /// <param name="s">The header values to parse.</param>
        /// <returns>The new instance of the <see cref="IfNoneMatchHeader"/> class.</returns>
        [NotNull]
        public static IfNoneMatchHeader Parse([NotNull] [ItemNotNull] IEnumerable<string> s)
        {
            return Parse(s, EntityTagComparer.Strong);
        }

        /// <summary>
        /// Parses the header string to get a new instance of the <see cref="IfNoneMatchHeader"/> class.
        /// </summary>
        /// <param name="s">The header values to parse.</param>
        /// <param name="etagComparer">The entity tag comparer used for the <see cref="IsMatch"/> function.</param>
        /// <returns>The new instance of the <see cref="IfNoneMatchHeader"/> class.</returns>
        [NotNull]
        public static IfNoneMatchHeader Parse([NotNull][ItemNotNull] IEnumerable<string> s, [NotNull] EntityTagComparer etagComparer)
        {
            var result = new List<EntityTag>();
            foreach (var etag in s)
            {
                if (etag == "*")
                {
                    return new IfNoneMatchHeader();
                }

                result.AddRange(EntityTag.Parse(etag));
            }

            if (result.Count == 0)
            {
                return new IfNoneMatchHeader();
            }

            return new IfNoneMatchHeader(result, etagComparer);
        }

        /// <summary>
        /// Returns a value that indicates whether the <paramref name="etag"/> is not specified in the <c>If-None-Match</c> header.
        /// </summary>
        /// <param name="etag">The entity tag to search for.</param>
        /// <returns><see langword="true"/> when the <paramref name="etag"/> was not found.</returns>
        public bool IsMatch(EntityTag? etag)
        {
            if (_etags == null)
            {
                return false;
            }

            if (etag == null)
            {
                return true;
            }

            return !_etags.Contains(etag.Value);
        }
    }
}
