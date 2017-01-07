using System.Collections.Generic;

namespace FubarDev.WebDavServer.Model
{
    public class DepthComparer : IComparer<Depth>, IEqualityComparer<Depth>
    {
        public static DepthComparer Default { get; } = new DepthComparer();

        public int Compare(Depth x, Depth y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            return x.OrderValue.CompareTo(y.OrderValue);
        }

        public bool Equals(Depth x, Depth y)
        {
            return Compare(x, y) == 0;
        }

        public int GetHashCode(Depth obj)
        {
            return obj?.OrderValue.GetHashCode() ?? 0;
        }
    }
}
