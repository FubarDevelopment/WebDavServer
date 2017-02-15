// <copyright file="RangeItem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Text.RegularExpressions;

namespace FubarDev.WebDavServer.Model
{
    /// <summary>
    /// Range for a HTTP request or response
    /// </summary>
    public struct RangeItem
    {
        private static readonly Regex _rangePattern = new Regex(@"^((\d+)-(\d+))|((\d+)-)|(-(\d+))|(\d+)$", RegexOptions.CultureInvariant);

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeItem"/> struct.
        /// </summary>
        /// <param name="from">From byte</param>
        /// <param name="to">To byte</param>
        public RangeItem(long? from, long? to)
        {
            From = from;
            To = to;
        }

        /// <summary>
        /// Gets the start position
        /// </summary>
        public long? From { get; }

        /// <summary>
        /// Gets the end position
        /// </summary>
        public long? To { get; }

        /// <summary>
        /// Parses a <see cref="RangeItem"/> from a string
        /// </summary>
        /// <remarks>
        /// Allowed are: "*", "from-", "-to", "from-to"
        /// </remarks>
        /// <param name="rangeItem">The string to parse</param>
        /// <returns>The new <see cref="RangeItem"/></returns>
        public static RangeItem Parse(string rangeItem)
        {
            if (rangeItem == "*")
                return default(RangeItem);
            var match = _rangePattern.Match(rangeItem);
            if (!match.Success)
                throw new ArgumentOutOfRangeException(nameof(rangeItem));
            var s = match.Groups[8].Value;
            if (!string.IsNullOrEmpty(s))
            {
                var v = Convert.ToInt64(s, 10);
                return new RangeItem(v, v);
            }

            s = match.Groups[7].Value;
            if (!string.IsNullOrEmpty(s))
            {
                var v = Convert.ToInt64(s, 10);
                return new RangeItem(null, v);
            }

            s = match.Groups[5].Value;
            if (!string.IsNullOrEmpty(s))
            {
                var v = Convert.ToInt64(s, 10);
                return new RangeItem(v, null);
            }

            var from = Convert.ToInt64(match.Groups[2].Value, 10);
            var to = Convert.ToInt64(match.Groups[3].Value, 10);
            return new RangeItem(from, to);
        }

        /// <summary>
        /// Returns the textual representation of the HTTP range item
        /// </summary>
        /// <returns>the textual representation of the HTTP range item</returns>
        public override string ToString()
        {
            if (From.HasValue || To.HasValue)
                return $"{From}-{To}";
            return "*";
        }

        public NormalizedRangeItem Normalize(long totalLength)
        {
            if (!From.HasValue && !To.HasValue)
                return new NormalizedRangeItem(0, totalLength - 1);
            if (From.HasValue && To.HasValue)
                return new NormalizedRangeItem(From.Value, To.Value);
            if (From.HasValue)
                return new NormalizedRangeItem(From.Value, totalLength - 1);
            return new NormalizedRangeItem(totalLength - To.Value, totalLength - 1);
        }
    }
}
