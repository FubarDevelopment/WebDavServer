// <copyright file="Range.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model
{
    /// <summary>
    /// This class encapsualtes a HTTP range
    /// </summary>
    public class Range
    {
        private static readonly char[] _splitEqualChar = { '=' };

        /// <summary>
        /// Initializes a new instance of the <see cref="Range"/> class.
        /// </summary>
        /// <param name="unit">The unit of the range (currently only <code>bytes</code> is allowed)</param>
        /// <param name="rangeItems">The HTTP range items for this range</param>
        public Range([NotNull] string unit, params RangeItem[] rangeItems)
            : this(unit, false, rangeItems)
        {
        }

        private Range([NotNull] string unit, bool ignoreInvalidUnit, params RangeItem[] rangeItems)
        {
            if (!ignoreInvalidUnit && !string.IsNullOrEmpty(unit) && unit != "bytes")
                throw new NotSupportedException();
            Unit = string.IsNullOrEmpty(unit) ? "bytes" : unit;
            RangeItems = rangeItems.ToList();
        }

        /// <summary>
        /// Gets the unit for this HTTP range
        /// </summary>
        [NotNull]
        public string Unit { get; }

        /// <summary>
        /// Gets the HTTP range items
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<RangeItem> RangeItems { get; }

        /// <summary>
        /// Parses a <paramref name="range"/> into a new <see cref="Range"/> instance
        /// </summary>
        /// <remarks>
        /// The range must be in the form <code>unit=(range)+</code>
        /// </remarks>
        /// <param name="range">The range to parse</param>
        /// <returns>The new <see cref="Range"/></returns>
        public static Range Parse(string range)
        {
            var parts = range.Split(_splitEqualChar, 2);
            var unit = parts[0];
            var rangeItems = parts[1].Split(',').Select(x => RangeItem.Parse(x.Trim())).ToArray();
            return new Range(unit, true, rangeItems);
        }

        /// <summary>
        /// Returns the textual representation of this range.
        /// </summary>
        /// <remarks>
        /// The return value of this function can be parsed using <see cref="Parse"/>.
        /// </remarks>
        /// <returns>The textual representation of this range</returns>
        public override string ToString()
        {
            return $"{Unit}={string.Join(",", RangeItems.Select(x => x.ToString()))}";
        }

        /// <summary>
        /// Returns the textual representation of a single <see cref="RangeItem"/>
        /// </summary>
        /// <remarks>
        /// The return value of this function looks like <code>unit range/length</code>
        /// </remarks>
        /// <param name="rangeItem">The <see cref="RangeItem"/> to get the textual representation for</param>
        /// <returns>The textual representation of <paramref name="rangeItem"/></returns>
        public virtual string ToString([NotNull] RangeItem rangeItem)
        {
            return ToString(rangeItem, null);
        }

        /// <summary>
        /// Returns the textual representation of a single <see cref="RangeItem"/>
        /// </summary>
        /// <remarks>
        /// The return value of this function looks like <code>unit range/length</code>
        /// </remarks>
        /// <param name="rangeItem">The <see cref="RangeItem"/> to get the textual representation for</param>
        /// <param name="length">The length value to be used in the textual representation</param>
        /// <returns>The textual representation of <paramref name="rangeItem"/></returns>
        public virtual string ToString([NotNull] RangeItem rangeItem, long? length)
        {
            return $"{Unit} {rangeItem}/{(length.HasValue ? length.Value.ToString(CultureInfo.InvariantCulture) : "*")}";
        }

        /// <summary>
        /// Normalize all byte ranges using the specified <paramref name="totalLength"/>
        /// </summary>
        /// <param name="totalLength">The length of the resource</param>
        /// <returns>The list of normalized byte ranges</returns>
        public IReadOnlyList<NormalizedRangeItem> Normalize(long totalLength)
        {
            var rangeItems = RangeItems.Select(x => x.Normalize(totalLength))
                .OrderBy(x => x.From).ThenBy(x => x.To);
            var result = new List<NormalizedRangeItem>();
            NormalizedRangeItem currentRangeItem = null;
            long currentTo = 0;
            foreach (var rangeItem in rangeItems)
            {
                if (currentRangeItem == null)
                {
                    currentRangeItem = rangeItem;
                    currentTo = rangeItem.To;
                }
                else
                {
                    var currentFrom = rangeItem.From;
                    if (currentFrom <= (currentTo + 1))
                    {
                        currentRangeItem = new NormalizedRangeItem(currentRangeItem.From, rangeItem.To);
                    }
                    else
                    {
                        result.Add(currentRangeItem);
                        currentRangeItem = rangeItem;
                        currentTo = rangeItem.To;
                    }
                }
            }

            if (currentRangeItem != null)
                result.Add(currentRangeItem);
            return result;
        }
    }
}
