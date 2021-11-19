// <copyright file="DepthHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;

namespace FubarDev.WebDavServer.Model.Headers
{
    /// <summary>
    /// The representation of the <c>Depth</c> header.
    /// </summary>
    public class DepthHeader : IComparable<DepthHeader>, IEquatable<DepthHeader>
    {
        /// <summary>
        /// Gets the default <c>0</c> depth header.
        /// </summary>
        public static readonly DepthHeader Zero = new DepthHeader("0", 0, activelockDepth.Item0);

        /// <summary>
        /// Gets the default <c>1</c> depth header.
        /// </summary>
        public static readonly DepthHeader One = new DepthHeader("1", 1, activelockDepth.Item1);

        /// <summary>
        /// Gets the default <c>infinity</c> depth header.
        /// </summary>
        public static readonly DepthHeader Infinity = new DepthHeader("infinity", int.MaxValue, activelockDepth.infinity);

        private DepthHeader(string value, int orderValue, activelockDepth xmlValue)
        {
            Value = value;
            OrderValue = orderValue;
            XmlValue = xmlValue;
        }

        /// <summary>
        /// Gets the ordinal value of the depth represented by this depth header instance.
        /// </summary>
        public int OrderValue { get; }

        /// <summary>
        /// Gets the textual value of the depth header as given in the HTTP header.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the object used in XML de-/serialization.
        /// </summary>
        public activelockDepth XmlValue { get; }

        /// <summary>
        /// Compares two depth headers for their equality.
        /// </summary>
        /// <param name="x">The first depth header to compare.</param>
        /// <param name="y">The second depth header to compare.</param>
        /// <returns><see langword="true"/> when both depth headers are of equal value.</returns>
        public static bool operator ==(DepthHeader x, DepthHeader y)
        {
            return DepthHeaderComparer.Default.Equals(x, y);
        }

        /// <summary>
        /// Compares two depth headers for their inequality.
        /// </summary>
        /// <param name="x">The first depth header to compare.</param>
        /// <param name="y">The second depth header to compare.</param>
        /// <returns><see langword="true"/> when both depth headers are not of equal value.</returns>
        public static bool operator !=(DepthHeader x, DepthHeader y)
        {
            return !DepthHeaderComparer.Default.Equals(x, y);
        }

        /// <summary>
        /// Compares if the <paramref name="x"/> depth header is of greater ordinal value than the <paramref name="y"/> depth header.
        /// </summary>
        /// <param name="x">The first depth header to compare.</param>
        /// <param name="y">The second depth header to compare.</param>
        /// <returns><see langword="true"/> when the <paramref name="x"/> depth header is of greater ordinal value than the <paramref name="y"/> depth header.</returns>
        public static bool operator >(DepthHeader x, DepthHeader y)
        {
            return DepthHeaderComparer.Default.Compare(x, y) > 0;
        }

        /// <summary>
        /// Compares if the <paramref name="x"/> depth header is of lower ordinal value than the <paramref name="y"/> depth header.
        /// </summary>
        /// <param name="x">The first depth header to compare.</param>
        /// <param name="y">The second depth header to compare.</param>
        /// <returns><see langword="true"/> when the <paramref name="x"/> depth header is of lower ordinal value than the <paramref name="y"/> depth header.</returns>
        public static bool operator <(DepthHeader x, DepthHeader y)
        {
            return DepthHeaderComparer.Default.Compare(x, y) < 0;
        }

        /// <summary>
        /// Compares if the <paramref name="x"/> depth header is of the same or greater ordinal value than the <paramref name="y"/> depth header.
        /// </summary>
        /// <param name="x">The first depth header to compare.</param>
        /// <param name="y">The second depth header to compare.</param>
        /// <returns><see langword="true"/> when the <paramref name="x"/> depth header is of the same or greater ordinal value than the <paramref name="y"/> depth header.</returns>
        public static bool operator >=(DepthHeader x, DepthHeader y)
        {
            return DepthHeaderComparer.Default.Compare(x, y) >= 0;
        }

        /// <summary>
        /// Compares if the <paramref name="x"/> depth header is of the same or lower ordinal value than the <paramref name="y"/> depth header.
        /// </summary>
        /// <param name="x">The first depth header to compare.</param>
        /// <param name="y">The second depth header to compare.</param>
        /// <returns><see langword="true"/> when the <paramref name="x"/> depth header is of the same or lower ordinal value than the <paramref name="y"/> depth header.</returns>
        public static bool operator <=(DepthHeader x, DepthHeader y)
        {
            return DepthHeaderComparer.Default.Compare(x, y) <= 0;
        }

        /// <summary>
        /// Parses the given depth header value and returns the corresponding <see cref="DepthHeader"/> instance.
        /// </summary>
        /// <param name="depth">The depth header to parse.</param>
        /// <returns>The corresponding <see cref="DepthHeader"/>.</returns>
        public static DepthHeader Parse(string depth)
        {
            return Parse(depth, Infinity);
        }

        /// <summary>
        /// Parses the given depth header value and returns the corresponding <see cref="DepthHeader"/> instance.
        /// </summary>
        /// <param name="depth">The depth header to parse.</param>
        /// <param name="defaultDepth">The default depth header to use when the <paramref name="depth"/> is empty.</param>
        /// <returns>The corresponding <see cref="DepthHeader"/>.</returns>
        public static DepthHeader Parse(string depth, DepthHeader defaultDepth)
        {
            if (!TryParse(depth, defaultDepth, out var result))
            {
                throw new ArgumentException("Argument must be one of \"0\", \"1\", or \"infinity\"", nameof(depth));
            }

            return result;
        }

        /// <summary>
        /// Tries to parse the depth header value.
        /// </summary>
        /// <param name="depthText">The depth header value.</param>
        /// <param name="depth">The found depth header.</param>
        /// <returns><c>true</c> when the value could be parsed.</returns>
        public static bool TryParse(string depthText, out DepthHeader depth)
        {
            return TryParse(depthText, Infinity, out depth);
        }

        /// <summary>
        /// Tries to parse the depth header value.
        /// </summary>
        /// <param name="depthText">The depth header value.</param>
        /// <param name="defaultDepth">The default depth header to use when the <paramref name="depthText"/> is empty.</param>
        /// <param name="depth">The found depth header.</param>
        /// <returns><c>true</c> when the value could be parsed.</returns>
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

        /// <inheritdoc />
        public bool Equals(DepthHeader other)
        {
            return DepthHeaderComparer.Default.Equals(this, other);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            Debug.Assert(obj != null, "obj != null");
            return DepthHeaderComparer.Default.Equals(this, (DepthHeader)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return DepthHeaderComparer.Default.GetHashCode(this);
        }

        /// <inheritdoc />
        public int CompareTo(DepthHeader other)
        {
            return DepthHeaderComparer.Default.Compare(this, other);
        }
    }
}
