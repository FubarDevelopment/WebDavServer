// <copyright file="EntityTagComparer.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FubarDev.WebDavServer.Model.Headers
{
    /// <summary>
    /// A comparer for entity tags.
    /// </summary>
    public class EntityTagComparer : IEqualityComparer<EntityTag>
    {
        private readonly bool _useStrongComparison;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTagComparer"/> class.
        /// </summary>
        /// <param name="useStrongComparison">Indicates whether strong comparison has to be used.</param>
        private EntityTagComparer(bool useStrongComparison)
        {
            _useStrongComparison = useStrongComparison;
        }

        /// <summary>
        /// Gets a default strong entity tag comparer.
        /// </summary>
        public static EntityTagComparer Strong { get; } = new EntityTagComparer(true);

        /// <summary>
        /// Gets a default weak entity tag comparer.
        /// </summary>
        public static EntityTagComparer Weak { get; } = new EntityTagComparer(false);

        /// <inheritdoc />
        public bool Equals(EntityTag x, EntityTag y)
        {
            if (_useStrongComparison)
            {
                return x.Value == y.Value && x.IsWeak == y.IsWeak && !x.IsWeak;
            }

            return x.Value == y.Value;
        }

        /// <inheritdoc />
        public int GetHashCode(EntityTag obj)
        {
            unchecked
            {
                var result = obj.Value.GetHashCode();
                if (_useStrongComparison)
                {
                    result ^= 137 * obj.IsWeak.GetHashCode();
                }

                return result;
            }
        }
    }
}
