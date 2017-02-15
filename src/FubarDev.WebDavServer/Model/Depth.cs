// <copyright file="Depth.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;

namespace FubarDev.WebDavServer.Model
{
    public struct Depth : IComparable<Depth>, IEquatable<Depth>
    {
        public static readonly Depth Zero = new Depth("0", 0, activelockDepth.Item0);
        public static readonly Depth One = new Depth("1", 1, activelockDepth.Item1);
        public static readonly Depth Infinity = new Depth("infinity", int.MaxValue, activelockDepth.infinity);

        private Depth(string value, int orderValue, activelockDepth xmlValue)
        {
            Value = value;
            OrderValue = orderValue;
            XmlValue = xmlValue;
        }

        public int OrderValue { get; }

        public string Value { get; }

        public activelockDepth XmlValue { get; }

        public static bool operator ==(Depth x, Depth y)
        {
            return DepthComparer.Default.Equals(x, y);
        }

        public static bool operator !=(Depth x, Depth y)
        {
            return !DepthComparer.Default.Equals(x, y);
        }

        public static bool operator >(Depth x, Depth y)
        {
            return DepthComparer.Default.Compare(x, y) > 0;
        }

        public static bool operator <(Depth x, Depth y)
        {
            return DepthComparer.Default.Compare(x, y) < 0;
        }

        public static bool operator >=(Depth x, Depth y)
        {
            return DepthComparer.Default.Compare(x, y) >= 0;
        }

        public static bool operator <=(Depth x, Depth y)
        {
            return DepthComparer.Default.Compare(x, y) <= 0;
        }

        public static Depth Parse(string depth)
        {
            return Parse(depth, Infinity);
        }

        public static Depth Parse(string depth, Depth defaultDepth)
        {
            Depth result;
            if (!TryParse(depth, defaultDepth, out result))
                throw new ArgumentException("Argument must be one of \"0\", \"1\", or \"infinity\"", nameof(depth));
            return result;
        }

        public static bool TryParse(string depthText, out Depth depth)
        {
            return TryParse(depthText, Infinity, out depth);
        }

        public static bool TryParse(string depthText, Depth defaultDepth, out Depth depth)
        {
            switch (depthText)
            {
                case "0":
                    depth = Zero;
                    return true;
                case "1":
                    depth = One;
                    return true;
                case "infinity":
                    depth = Infinity;
                    return true;
            }

            depth = defaultDepth;
            return string.IsNullOrEmpty(depthText);
        }

        public bool Equals(Depth other)
        {
            return DepthComparer.Default.Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            Debug.Assert(obj != null, "obj != null");
            return DepthComparer.Default.Equals(this, (Depth)obj);
        }

        public override int GetHashCode()
        {
            return DepthComparer.Default.GetHashCode(this);
        }

        public int CompareTo(Depth other)
        {
            return DepthComparer.Default.Compare(this, other);
        }
    }
}
