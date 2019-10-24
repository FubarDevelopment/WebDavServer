// <copyright file="RangeHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FubarDev.WebDavServer.Model.Headers
{
    /// <summary>
    /// This class encapsulates a HTTP range.
    /// </summary>
    public class RangeHeader
    {
        private static readonly char[] _splitEqualChar = { '=' };

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeHeader"/> class.
        /// </summary>
        /// <param name="unit">The unit of the range (currently only <c>bytes</c> is allowed).</param>
        /// <param name="rangeItems">The HTTP range items for this range.</param>
        public RangeHeader(string unit, params RangeHeaderItem[] rangeItems)
            : this(unit, false, rangeItems)
        {
        }

        private RangeHeader(string unit, bool ignoreInvalidUnit, IReadOnlyCollection<RangeHeaderItem> rangeItems)
        {
            if (!ignoreInvalidUnit && !string.IsNullOrEmpty(unit) && unit != "bytes")
            {
                throw new NotSupportedException();
            }

            Unit = string.IsNullOrEmpty(unit) ? "bytes" : unit;
            RangeItems = rangeItems.ToList();
        }

        /// <summary>
        /// Gets the unit for this HTTP range.
        /// </summary>
        public string Unit { get; }

        /// <summary>
        /// Gets the HTTP range items.
        /// </summary>
        public IReadOnlyCollection<RangeHeaderItem> RangeItems { get; }

        /// <summary>
        /// Parses a <paramref name="range"/> into a new <see cref="RangeHeader"/> instance.
        /// </summary>
        /// <remarks>
        /// The range must be in the form <c>unit=(range)+</c>.
        /// </remarks>
        /// <param name="range">The range to parse.</param>
        /// <returns>The new <see cref="RangeHeader"/>.</returns>
        public static RangeHeader Parse(string range)
        {
            return Parse(range.Split(','));
        }

        /// <summary>
        /// Parses the <paramref name="ranges"/> into a new <see cref="RangeHeader"/> instance.
        /// </summary>
        /// <remarks>
        /// The range must be in the form <c>unit=(range)+</c>.
        /// </remarks>
        /// <param name="ranges">The ranges to parse.</param>
        /// <returns>The new <see cref="RangeHeader"/>.</returns>
        public static RangeHeader Parse(IEnumerable<string> ranges)
        {
            var rangeItems = new List<RangeHeaderItem>();
            var firstEntry = true;
            string? unit = null;
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
                    var item = RangeHeaderItem.Parse(rangeValueItem);
                    rangeItems.Add(item);
                }
            }

            return new RangeHeader(unit ?? "bytes", true, rangeItems);
        }

        /// <summary>
        /// Returns the textual representation of this range.
        /// </summary>
        /// <remarks>
        /// The return value of this function can be parsed using <see cref="Parse(string)"/>.
        /// </remarks>
        /// <returns>The textual representation of this range.</returns>
        public override string ToString()
        {
            return $"{Unit}={string.Join(",", RangeItems.Select(x => x.ToString()))}";
        }

        /// <summary>
        /// Returns the textual representation of a single <see cref="RangeHeaderItem"/>.
        /// </summary>
        /// <remarks>
        /// The return value of this function looks like <c>unit range/length</c>.
        /// </remarks>
        /// <param name="rangeItem">The <see cref="RangeHeaderItem"/> to get the textual representation for.</param>
        /// <returns>The textual representation of <paramref name="rangeItem"/>.</returns>
        public virtual string ToString(RangeHeaderItem rangeItem)
        {
            return ToString(rangeItem, null);
        }

        /// <summary>
        /// Returns the textual representation of a single <see cref="RangeHeaderItem"/>.
        /// </summary>
        /// <remarks>
        /// The return value of this function looks like <c>unit range/length</c>.
        /// </remarks>
        /// <param name="rangeItem">The <see cref="RangeHeaderItem"/> to get the textual representation for.</param>
        /// <param name="length">The length value to be used in the textual representation.</param>
        /// <returns>The textual representation of <paramref name="rangeItem"/>.</returns>
        public virtual string ToString(RangeHeaderItem rangeItem, long? length)
        {
            return $"{Unit} {rangeItem}/{length?.ToString(CultureInfo.InvariantCulture) ?? "*"}";
        }

        /// <summary>
        /// Normalize all byte ranges using the specified <paramref name="totalLength"/>.
        /// </summary>
        /// <param name="totalLength">The length of the resource.</param>
        /// <returns>The list of normalized byte ranges.</returns>
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
            {
                result.Add(currentRangeItem.Value);
            }

            return result;
        }
    }
}
