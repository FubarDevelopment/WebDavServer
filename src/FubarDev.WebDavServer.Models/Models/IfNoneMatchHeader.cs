// <copyright file="IfNoneMatchHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Models
{
    /// <summary>
    /// Class that represents the HTTP <c>If-None-Match</c> header.
    /// </summary>
    public class IfNoneMatchHeader
    {
        private readonly EntityTagComparer _etagComparer;
        private readonly Dictionary<EntityTag, List<EntityTag>>? _etags;

        /// <summary>
        /// Initializes a new instance of the <see cref="IfNoneMatchHeader"/> class.
        /// </summary>
        /// <param name="etags">The entity tags to match.</param>
        /// <param name="etagComparer">The entity comparer to use.</param>
        private IfNoneMatchHeader(IEnumerable<EntityTag> etags, EntityTagComparer etagComparer)
        {
            _etagComparer = etagComparer;
            _etags = new Dictionary<EntityTag, List<EntityTag>>(etagComparer);
            foreach (var etag in etags)
            {
                var key = etag.AsWeak();
                if (_etags.TryGetValue(key, out var found))
                {
                    found.Add(etag);
                }
                else
                {
                    _etags.Add(key, new List<EntityTag>() { etag });
                }
            }
        }

        private IfNoneMatchHeader()
        {
            _etagComparer = EntityTagComparer.Strong;
            _etags = null;
        }

        /// <summary>
        /// Parses the header string to get a new instance of the <see cref="IfNoneMatchHeader"/> class.
        /// </summary>
        /// <param name="s">The header string to parse.</param>
        /// <returns>The new instance of the <see cref="IfNoneMatchHeader"/> class.</returns>
        public static IfNoneMatchHeader Parse(string? s)
        {
            return Parse(s, EntityTagComparer.Strong);
        }

        /// <summary>
        /// Parses the header string to get a new instance of the <see cref="IfNoneMatchHeader"/> class.
        /// </summary>
        /// <param name="s">The header string to parse.</param>
        /// <param name="etagComparer">The entity tag comparer used for the <see cref="IsMatch"/> function.</param>
        /// <returns>The new instance of the <see cref="IfNoneMatchHeader"/> class.</returns>
        public static IfNoneMatchHeader Parse(string? s, EntityTagComparer etagComparer)
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
        public static IfNoneMatchHeader Parse(IEnumerable<string> s)
        {
            return Parse(s, EntityTagComparer.Strong);
        }

        /// <summary>
        /// Parses the header string to get a new instance of the <see cref="IfNoneMatchHeader"/> class.
        /// </summary>
        /// <param name="s">The header values to parse.</param>
        /// <param name="etagComparer">The entity tag comparer used for the <see cref="IsMatch"/> function.</param>
        /// <returns>The new instance of the <see cref="IfNoneMatchHeader"/> class.</returns>
        public static IfNoneMatchHeader Parse(IEnumerable<string> s, EntityTagComparer etagComparer)
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

            if (etag is not { } entityTag)
            {
                return true;
            }

            if (!_etags.TryGetValue(entityTag.AsWeak(), out var found))
            {
                return true;
            }

            return !found.Any(item => _etagComparer.Equals(entityTag, item));
        }
    }
}
