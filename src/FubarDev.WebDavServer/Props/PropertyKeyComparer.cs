// <copyright file="PropertyKeyComparer.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace FubarDev.WebDavServer.Props
{
    /// <summary>
    /// Comparer for the <see cref="PropertyKey"/>
    /// </summary>
    public class PropertyKeyComparer : IEqualityComparer<PropertyKey>
    {
        /// <summary>
        /// Gets the default <see cref="PropertyKey"/> comparer
        /// </summary>
        public static PropertyKeyComparer Default { get; } = new PropertyKeyComparer();

        /// <inheritdoc />
        public bool Equals(PropertyKey x, PropertyKey y)
        {
            return ReferenceEquals(x.Name, y.Name)
                   && string.Equals(x.Language, y.Language, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public int GetHashCode(PropertyKey obj)
        {
            unchecked
            {
                return (obj.Name.GetHashCode() * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Language);
            }
        }
    }
}
