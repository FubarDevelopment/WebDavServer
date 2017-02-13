// <copyright file="DepthComparer.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FubarDev.WebDavServer.Model
{
    public class DepthComparer : IComparer<Depth>, IEqualityComparer<Depth>
    {
        public static DepthComparer Default { get; } = new DepthComparer();

        public int Compare(Depth x, Depth y)
        {
            return x.OrderValue.CompareTo(y.OrderValue);
        }

        public bool Equals(Depth x, Depth y)
        {
            return Compare(x, y) == 0;
        }

        public int GetHashCode(Depth obj)
        {
            return obj.OrderValue.GetHashCode();
        }
    }
}
