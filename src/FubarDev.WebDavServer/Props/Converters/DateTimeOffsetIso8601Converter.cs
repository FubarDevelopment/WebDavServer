// <copyright file="DateTimeOffsetIso8601Converter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props.Converters
{
    public class DateTimeOffsetIso8601Converter : IPropertyConverter<DateTimeOffset>
    {
        private static readonly char[] _dateTimeSeparators = { 'T', ' ', 't' };
        private static readonly char[] _timeZoneSeparators = { '+', '-', 'Z', 'z' };

        /// <summary>
        /// Parses a string as a ISO 8601 date
        /// </summary>
        /// <param name="s">The string to parse</param>
        /// <returns>The parsed date</returns>
        public static DateTimeOffset Parse([NotNull] string s)
        {
            var dateTimeSeparatorIndex = s.IndexOfAny(_dateTimeSeparators);
            if (dateTimeSeparatorIndex == -1)
                throw new FormatException($"{s} is not a valid ISO 8601 format (no recognized date/time separator)");

            var datePart = s.Substring(0, dateTimeSeparatorIndex);
            var timeWithTimeZonePart = s.Substring(dateTimeSeparatorIndex + 1);

            var timeZoneSeparatorIndex = timeWithTimeZonePart.IndexOfAny(_timeZoneSeparators);
            if (timeZoneSeparatorIndex == -1)
            {
                // No time zone
                throw new FormatException($"{s} is not a valid ISO 8601 format (no time zone given)");
            }

            var timePart = timeWithTimeZonePart.Substring(0, timeZoneSeparatorIndex);
            var timeZonePart = timeWithTimeZonePart.Substring(timeZoneSeparatorIndex);

            var offset = ParseOffset(timeZonePart);
            var time = TimeSpan.Parse(timePart);
            var date = DateTime.ParseExact(datePart, "yyyy'-'MM'-'dd", CultureInfo.InvariantCulture);
            var dateTime = date + time;

            return new DateTimeOffset(dateTime, offset);
        }

        /// <inheritdoc />
        public bool IsValidValue(DateTimeOffset value)
        {
            return value != DateTimeOffset.MinValue;
        }

        /// <inheritdoc />
        public DateTimeOffset FromElement(XElement element)
        {
            return Parse(element.Value);
        }

        /// <inheritdoc />
        public XElement ToElement(XName name, DateTimeOffset value)
        {
            if (value == DateTimeOffset.MinValue)
                return new XElement(name);

            var format = new StringBuilder("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
            if (value.Millisecond != 0)
            {
                format.Append("'.'fff");
            }

            format.Append(value.Offset == TimeSpan.Zero ? "'Z'" : "zzz");

            return new XElement(name, value.ToString(format.ToString()));
        }

        private static TimeSpan ParseOffset(string timeZonePart)
        {
            if (string.Equals(timeZonePart, "Z", StringComparison.OrdinalIgnoreCase))
            {
                // UTC
                return TimeSpan.Zero;
            }

            if (timeZonePart.StartsWith("+", StringComparison.OrdinalIgnoreCase))
            {
                // Positive
                return TimeSpan.Parse(timeZonePart.Substring(1));
            }

            // Negative
            return TimeSpan.Parse(timeZonePart);
        }
    }
}
