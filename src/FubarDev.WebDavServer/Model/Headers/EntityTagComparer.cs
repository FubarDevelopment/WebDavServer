// <copyright file="EntityTagComparer.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model.Headers
{
    public class EntityTagComparer : IEqualityComparer<EntityTag>
    {
        private readonly bool _useStrongComparison;

        private EntityTagComparer(bool useStrongComparison)
        {
            _useStrongComparison = useStrongComparison;
        }

        [NotNull]
        public static EntityTagComparer Strong { get; } = new EntityTagComparer(true);

        [NotNull]
        public static EntityTagComparer Weak { get; } = new EntityTagComparer(false);

        public bool Equals(EntityTag x, EntityTag y)
        {
            if (_useStrongComparison)
                return x.Value == y.Value && x.IsWeak == y.IsWeak && !x.IsWeak;
            return x.Value == y.Value;
        }

        public int GetHashCode(EntityTag obj)
        {
            unchecked
            {
                var result = obj.Value.GetHashCode();
                if (_useStrongComparison)
                    result ^= 137 * obj.IsWeak.GetHashCode();
                return result;
            }
        }
    }
}
