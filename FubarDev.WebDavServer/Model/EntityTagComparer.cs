// <copyright file="EntityTagComparer.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FubarDev.WebDavServer.Model
{
    public class EntityTagComparer : IEqualityComparer<EntityTag>
    {
        public static EntityTagComparer Default { get; } = new EntityTagComparer();

        public bool Equals(EntityTag x, EntityTag y)
        {
            return x.Value == y.Value && x.IsWeak == y.IsWeak;
        }

        public int GetHashCode(EntityTag obj)
        {
            unchecked
            {
                var result = obj.Value.GetHashCode();
                result ^= 137 * obj.IsWeak.GetHashCode();
                return result;
            }
        }
    }
}
