// <copyright file="DepthHeaderComparer.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model.Headers
{
    public class DepthHeaderComparer : IComparer<DepthHeader>, IEqualityComparer<DepthHeader>
    {
        [NotNull]
        public static DepthHeaderComparer Default { get; } = new DepthHeaderComparer();

        public int Compare(DepthHeader x, DepthHeader y)
        {
            return x.OrderValue.CompareTo(y.OrderValue);
        }

        public bool Equals(DepthHeader x, DepthHeader y)
        {
            return Compare(x, y) == 0;
        }

        public int GetHashCode(DepthHeader obj)
        {
            return obj.OrderValue.GetHashCode();
        }
    }
}
