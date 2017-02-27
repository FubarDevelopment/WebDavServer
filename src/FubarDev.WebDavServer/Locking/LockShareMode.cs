// <copyright file="LockShareMode.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// The lock share mode
    /// </summary>
    public struct LockShareMode : IEquatable<LockShareMode>
    {
        /// <summary>
        /// Gets the default <c>shared</c> lock share mode
        /// </summary>
        public static readonly LockShareMode Shared = new LockShareMode(SharedId, new lockscope()
        {
            ItemElementName = ItemChoiceType.shared,
            Item = new object(),
        });

        /// <summary>
        /// Gets the default <c>exclusive</c> lock share mode
        /// </summary>
        public static readonly LockShareMode Exclusive = new LockShareMode(ExclusiveId, new lockscope()
        {
            ItemElementName = ItemChoiceType.exclusive,
            Item = new object(),
        });

        private const string SharedId = "shared";
        private const string ExclusiveId = "exclusive";

        private LockShareMode([NotNull] string id, [NotNull] lockscope xmlValue)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            Name = WebDavXml.Dav + id;
            XmlValue = xmlValue;
        }

        /// <summary>
        /// Gets the XML name of the lock share mode
        /// </summary>
        [NotNull]
        public XName Name { get; }

        /// <summary>
        /// Gets the <see cref="lockscope"/> element for this lock share mode
        /// </summary>
        [NotNull]
        public lockscope XmlValue { get; }

        /// <summary>
        /// Compares two lock share modes for their equality
        /// </summary>
        /// <param name="x">The first lock share mode to compare</param>
        /// <param name="y">The second lock share mode to compare</param>
        /// <returns><see langword="true"/> when both lock share modes are of equal value</returns>
        public static bool operator ==(LockShareMode x, LockShareMode y)
        {
            return x.Name == y.Name;
        }

        /// <summary>
        /// Compares two lock share modes for their inequality
        /// </summary>
        /// <param name="x">The first lock share mode to compare</param>
        /// <param name="y">The second lock share mode to compare</param>
        /// <returns><see langword="true"/> when both lock share modes are not of equal value</returns>
        public static bool operator !=(LockShareMode x, LockShareMode y)
        {
            return x.Name != y.Name;
        }

        /// <summary>
        /// Parses the given lock share mode value and returns the corresponding <see cref="LockShareMode"/> instance.
        /// </summary>
        /// <param name="shareMode">The share mode to parse</param>
        /// <returns>The corresponding <see cref="LockShareMode"/></returns>
        public static LockShareMode Parse([NotNull] string shareMode)
        {
            if (shareMode == null)
                throw new ArgumentNullException(nameof(shareMode));

            switch (shareMode.ToLowerInvariant())
            {
                case SharedId:
                    return Shared;
                case ExclusiveId:
                    return Exclusive;
            }

            throw new ArgumentOutOfRangeException(nameof(shareMode), $"The share mode {shareMode} is not supported.");
        }

        /// <inheritdoc />
        public bool Equals(LockShareMode other)
        {
            return Name.Equals(other.Name);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is LockShareMode && Equals((LockShareMode)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
