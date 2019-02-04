// <copyright file="TimeoutHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

namespace FubarDev.WebDavServer.Model.Headers
{
    /// <summary>
    /// The HTTP <c>Timeout</c> header.
    /// </summary>
    public class TimeoutHeader
    {
        private static readonly char[] _unitValueSplitChar = { '-' };

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeoutHeader"/> class.
        /// </summary>
        /// <param name="values">The timeout values.</param>
        public TimeoutHeader(IReadOnlyCollection<TimeSpan> values)
        {
            Values = values;
        }

        /// <summary>
        /// Gets the timeout value for an infinite timeout.
        /// </summary>
        public static TimeSpan Infinite { get; } = TimeSpan.MaxValue;

        /// <summary>
        /// Gets the timeout values of the <c>Timeout</c> header.
        /// </summary>
        public IReadOnlyCollection<TimeSpan> Values { get; }

        /// <summary>
        /// Parses the header values to get a new instance of the <see cref="TimeoutHeader"/> class.
        /// </summary>
        /// <param name="args">The header values to parse.</param>
        /// <returns>The new instance of the <see cref="TimeoutHeader"/> class.</returns>
        public static TimeoutHeader Parse(IEnumerable<string> args)
        {
            var timespans = new List<TimeSpan>();
            foreach (var arg in args)
            {
                if (arg == "Infinite")
                {
                    timespans.Add(Infinite);
                }
                else
                {
                    var parts = arg.Split(_unitValueSplitChar, 2);
                    var unit = parts[0].Trim();
                    var value = parts[1].Trim();

                    switch (unit.ToLowerInvariant())
                    {
                        case "second":
                            timespans.Add(TimeSpan.FromSeconds(Convert.ToInt32(value, 10)));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(args), $"Unknown unit {unit}");
                    }
                }
            }

            return new TimeoutHeader(timespans);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var output = Values.Select(timeSpan => timeSpan == Infinite ? "Infinite" : $"Second-{timeSpan.TotalSeconds:F0}");
            return string.Join(",", output);
        }
    }
}
