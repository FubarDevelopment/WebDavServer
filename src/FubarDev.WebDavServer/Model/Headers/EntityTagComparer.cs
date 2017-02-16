// <copyright file="EntityTagComparer.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model.Headers
{
    public class EntityTagComparer : IEqualityComparer<EntityTag>
    {
        [NotNull]
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
