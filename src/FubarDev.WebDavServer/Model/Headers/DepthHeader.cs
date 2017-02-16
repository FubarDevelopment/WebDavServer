// <copyright file="DepthHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;

namespace FubarDev.WebDavServer.Model.Headers
{
    public struct DepthHeader : IComparable<DepthHeader>, IEquatable<DepthHeader>
    {
        public static readonly DepthHeader Zero = new DepthHeader("0", 0, activelockDepth.Item0);
        public static readonly DepthHeader One = new DepthHeader("1", 1, activelockDepth.Item1);
        public static readonly DepthHeader Infinity = new DepthHeader("infinity", int.MaxValue, activelockDepth.infinity);

        private DepthHeader(string value, int orderValue, activelockDepth xmlValue)
        {
            Value = value;
            OrderValue = orderValue;
            XmlValue = xmlValue;
        }

        public int OrderValue { get; }

        public string Value { get; }

        public activelockDepth XmlValue { get; }

        public static bool operator ==(DepthHeader x, DepthHeader y)
        {
            return DepthHeaderComparer.Default.Equals(x, y);
        }

        public static bool operator !=(DepthHeader x, DepthHeader y)
        {
            return !DepthHeaderComparer.Default.Equals(x, y);
        }

        public static bool operator >(DepthHeader x, DepthHeader y)
        {
            return DepthHeaderComparer.Default.Compare(x, y) > 0;
        }

        public static bool operator <(DepthHeader x, DepthHeader y)
        {
            return DepthHeaderComparer.Default.Compare(x, y) < 0;
        }

        public static bool operator >=(DepthHeader x, DepthHeader y)
        {
            return DepthHeaderComparer.Default.Compare(x, y) >= 0;
        }

        public static bool operator <=(DepthHeader x, DepthHeader y)
        {
            return DepthHeaderComparer.Default.Compare(x, y) <= 0;
        }

        public static DepthHeader Parse(string depth)
        {
            return Parse(depth, Infinity);
        }

        public static DepthHeader Parse(string depth, DepthHeader defaultDepth)
        {
            DepthHeader result;
            if (!TryParse(depth, defaultDepth, out result))
                throw new ArgumentException("Argument must be one of \"0\", \"1\", or \"infinity\"", nameof(depth));
            return result;
        }

        public static bool TryParse(string depthText, out DepthHeader depth)
        {
            return TryParse(depthText, Infinity, out depth);
        }

        public static bool TryParse(string depthText, DepthHeader defaultDepth, out DepthHeader depth)
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

        public bool Equals(DepthHeader other)
        {
            return DepthHeaderComparer.Default.Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            Debug.Assert(obj != null, "obj != null");
            return DepthHeaderComparer.Default.Equals(this, (DepthHeader)obj);
        }

        public override int GetHashCode()
        {
            return DepthHeaderComparer.Default.GetHashCode(this);
        }

        public int CompareTo(DepthHeader other)
        {
            return DepthHeaderComparer.Default.Compare(this, other);
        }
    }
}
