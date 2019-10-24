// <copyright file="IfMatchHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FubarDev.WebDavServer.Model.Headers
{
    /// <summary>
    /// Class that represents the HTTP <c>If-Match</c> header.
    /// </summary>
    public class IfMatchHeader
    {
        private readonly ISet<EntityTag>? _etags;

        /// <summary>
        /// Initializes a new instance of the <see cref="IfMatchHeader"/> class.
        /// </summary>
        public IfMatchHeader()
        {
            _etags = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IfMatchHeader"/> class.
        /// </summary>
        /// <param name="etags">The entity tags to match.</param>
        /// <param name="etagComparer">The entity comparer to use.</param>
        public IfMatchHeader(IEnumerable<EntityTag> etags, EntityTagComparer etagComparer)
        {
            _etags = new HashSet<EntityTag>(etags, etagComparer);
        }

        /// <summary>
        /// Parses the header string to get a new instance of the <see cref="IfMatchHeader"/> class.
        /// </summary>
        /// <param name="s">The header string to parse.</param>
        /// <returns>The new instance of the <see cref="IfMatchHeader"/> class.</returns>
        public static IfMatchHeader Parse(string? s)
        {
            return Parse(s, EntityTagComparer.Strong);
        }

        /// <summary>
        /// Parses the header string to get a new instance of the <see cref="IfMatchHeader"/> class.
        /// </summary>
        /// <param name="s">The header string to parse.</param>
        /// <param name="etagComparer">The entity tag comparer used for the <see cref="IsMatch"/> function.</param>
        /// <returns>The new instance of the <see cref="IfMatchHeader"/> class.</returns>
        public static IfMatchHeader Parse(string? s, EntityTagComparer etagComparer)
        {
            if (string.IsNullOrWhiteSpace(s) || s == "*")
            {
                return new IfMatchHeader();
            }

            return new IfMatchHeader(EntityTag.Parse(s), etagComparer);
        }

        /// <summary>
        /// Parses the header string to get a new instance of the <see cref="IfMatchHeader"/> class.
        /// </summary>
        /// <param name="s">The header values to parse.</param>
        /// <returns>The new instance of the <see cref="IfMatchHeader"/> class.</returns>
        public static IfMatchHeader Parse(IEnumerable<string> s)
        {
            return Parse(s, EntityTagComparer.Strong);
        }

        /// <summary>
        /// Parses the header string to get a new instance of the <see cref="IfMatchHeader"/> class.
        /// </summary>
        /// <param name="s">The header values to parse.</param>
        /// <param name="etagComparer">The entity tag comparer used for the <see cref="IsMatch"/> function.</param>
        /// <returns>The new instance of the <see cref="IfMatchHeader"/> class.</returns>
        public static IfMatchHeader Parse(IEnumerable<string> s, EntityTagComparer etagComparer)
        {
            var result = new List<EntityTag>();
            foreach (var etag in s)
            {
                if (etag == "*")
                {
                    return new IfMatchHeader();
                }

                result.AddRange(EntityTag.Parse(etag));
            }

            if (result.Count == 0)
            {
                return new IfMatchHeader();
            }

            return new IfMatchHeader(result, etagComparer);
        }

        /// <summary>
        /// Returns a value that indicates whether the <paramref name="etag"/> is specified in the <c>If-Match</c> header.
        /// </summary>
        /// <param name="etag">The entity tag to search for.</param>
        /// <returns><see langword="true"/> when the <paramref name="etag"/> was found.</returns>
        public bool IsMatch(EntityTag? etag)
        {
            if (_etags == null)
            {
                return true;
            }

            if (etag == null)
            {
                return false;
            }

            return _etags.Contains(etag.Value);
        }
    }
}
