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

        private Range([NotNull] string unit, bool ignoreInvalidUnit, IReadOnlyCollection<RangeItem> rangeItems)
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
        public IReadOnlyCollection<RangeItem> RangeItems { get; }

        /// <summary>
        /// Parses a <paramref name="range"/> into a new <see cref="Range"/> instance
        /// </summary>
        /// <remarks>
        /// The range must be in the form <code>unit=(range)+</code>
        /// </remarks>
        /// <param name="range">The range to parse</param>
        /// <returns>The new <see cref="Range"/></returns>
        [NotNull]
        public static Range Parse([NotNull] string range)
        {
            return Parse(range.Split(','));
        }

        /// <summary>
        /// Parses the <paramref name="ranges"/> into a new <see cref="Range"/> instance
        /// </summary>
        /// <remarks>
        /// The range must be in the form <code>unit=(range)+</code>
        /// </remarks>
        /// <param name="ranges">The ranges to parse</param>
        /// <returns>The new <see cref="Range"/></returns>
        [NotNull]
        public static Range Parse([NotNull][ItemNotNull] IEnumerable<string> ranges)
        {
            var rangeItems = new List<RangeItem>();
            var firstEntry = true;
            string unit = null;
            foreach (var range in ranges)
            {
                string rangeValue;
                if (firstEntry)
                {
                    var parts = range.Split(_splitEqualChar, 2);
                    if (parts.Length == 1)
                    {
                        rangeValue = parts[0];
                    }
                    else
                    {
                        unit = parts[0].TrimEnd();
                        rangeValue = parts[1].TrimStart();
                    }

                    firstEntry = false;
                }
                else
                {
                    rangeValue = range;
                }

                foreach (var rangeValueItem in rangeValue.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)))
                {
                    var item = RangeItem.Parse(rangeValueItem);
                    rangeItems.Add(item);
                }
            }

            return new Range(unit ?? "bytes", true, rangeItems);
        }

        /// <summary>
        /// Returns the textual representation of this range.
        /// </summary>
        /// <remarks>
        /// The return value of this function can be parsed using <see cref="Parse(string)"/>.
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
        public virtual string ToString(RangeItem rangeItem)
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
        public virtual string ToString(RangeItem rangeItem, long? length)
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
            NormalizedRangeItem? currentRangeItem = null;
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
                        currentRangeItem = new NormalizedRangeItem(currentRangeItem.Value.From, rangeItem.To);
                    }
                    else
                    {
                        result.Add(currentRangeItem.Value);
                        currentRangeItem = rangeItem;
                        currentTo = rangeItem.To;
                    }
                }
            }

            if (currentRangeItem != null)
                result.Add(currentRangeItem.Value);
            return result;
        }
    }
}
