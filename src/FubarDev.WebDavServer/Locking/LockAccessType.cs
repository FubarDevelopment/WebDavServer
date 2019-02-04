// <copyright file="LockAccessType.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// The lock access type.
    /// </summary>
    public struct LockAccessType : IEquatable<LockAccessType>
    {
        /// <summary>
        /// The default <c>write</c> lock access type.
        /// </summary>
        public static LockAccessType Write = new LockAccessType(WriteId, new locktype()
        {
            Item = new object(),
        });

        private const string WriteId = "write";

        private LockAccessType([NotNull] string id, [NotNull] locktype xmlValue)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            Name = WebDavXml.Dav + id;
            XmlValue = xmlValue;
        }

        /// <summary>
        /// Gets the XML name of the lock access type.
        /// </summary>
        [NotNull]
        public XName Name { get; }

        /// <summary>
        /// Gets the <see cref="locktype"/> element for this lock access type.
        /// </summary>
        [NotNull]
        public locktype XmlValue { get; }

        /// <summary>
        /// Compares two lock access types for their equality.
        /// </summary>
        /// <param name="x">The first lock access type to compare.</param>
        /// <param name="y">The second lock access type to compare.</param>
        /// <returns><see langword="true"/> when both lock access types are of equal value.</returns>
        public static bool operator ==(LockAccessType x, LockAccessType y)
        {
            return x.Name == y.Name;
        }

        /// <summary>
        /// Compares two lock access types for their inequality.
        /// </summary>
        /// <param name="x">The first lock access type to compare.</param>
        /// <param name="y">The second lock access type to compare.</param>
        /// <returns><see langword="true"/> when both lock access types are not of equal value.</returns>
        public static bool operator !=(LockAccessType x, LockAccessType y)
        {
            return x.Name != y.Name;
        }

        /// <summary>
        /// Parses the given lock access type value and returns the corresponding <see cref="LockAccessType"/> instance.
        /// </summary>
        /// <param name="accessType">The access type to parse.</param>
        /// <returns>The corresponding <see cref="LockAccessType"/>.</returns>
        public static LockAccessType Parse([NotNull] string accessType)
        {
            if (accessType == null)
            {
                throw new ArgumentNullException(nameof(accessType));
            }

            switch (accessType.ToLowerInvariant())
            {
                case WriteId:
                    return Write;
            }

            throw new ArgumentOutOfRangeException(nameof(accessType), $"The access type {accessType} is not supported.");
        }

        /// <inheritdoc />
        public bool Equals(LockAccessType other)
        {
            return Name.Equals(other.Name);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is LockAccessType && Equals((LockAccessType)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
