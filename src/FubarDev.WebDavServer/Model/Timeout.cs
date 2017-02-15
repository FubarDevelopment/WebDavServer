// <copyright file="Timeout.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

namespace FubarDev.WebDavServer.Model
{
    public class Timeout
    {
        private static readonly char[] _unitValueSplitChar = { '-' };

        public Timeout(IReadOnlyCollection<TimeSpan> values)
        {
            Values = values;
        }

        public Timeout(params TimeSpan[] values)
        {
            Values = values;
        }

        public static TimeSpan Infinite { get; } = TimeSpan.MaxValue;

        public IReadOnlyCollection<TimeSpan> Values { get; }

        public static Timeout Parse(string s)
        {
            return Parse(s.Split(',').Where(x => !string.IsNullOrEmpty(x)));
        }

        public static Timeout Parse(IEnumerable<string> args)
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

                    switch (unit)
                    {
                        case "Seconds":
                            timespans.Add(TimeSpan.FromSeconds(Convert.ToInt32(value, 10)));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(args), $"Unknown unit {unit}");
                    }
                }
            }

            return new Timeout(timespans);
        }
    }
}
