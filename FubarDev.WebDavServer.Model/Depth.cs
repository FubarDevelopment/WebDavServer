using System;

namespace FubarDev.WebDavServer.Model
{
    public class Depth : IComparable<Depth>, IEquatable<Depth>
    {
        public static readonly Depth Zero = new Depth("0", 0);
        public static readonly Depth One = new Depth("1", 1);
        public static readonly Depth Infinity = new Depth("infinity", int.MaxValue);

        private Depth(string value, int orderValue)
        {
            Value = value;
            OrderValue = orderValue;
        }

        public int OrderValue { get; }

        public string Value { get; }

        public static bool TryParse(string depthText, out Depth depth)
        {
            return TryParse(depthText, null, out depth);
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

        private static bool TryParse(string depthText, Depth defaultDepth, out Depth depth)
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

            if (string.IsNullOrEmpty(depthText) && defaultDepth != null)
            {
                depth = defaultDepth;
                return true;
            }

            depth = null;
            return false;
        }

        public bool Equals(Depth other)
        {
            return DepthComparer.Default.Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            return DepthComparer.Default.Equals(this, (Depth) obj);
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
