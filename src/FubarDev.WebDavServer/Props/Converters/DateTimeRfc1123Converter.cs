// <copyright file="DateTimeRfc1123Converter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Globalization;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props.Converters
{
    /// <summary>
    /// A property converter for RFC 1123 dates.
    /// </summary>
    public class DateTimeRfc1123Converter : IPropertyConverter<DateTime>
    {
        /// <summary>
        /// Parses a string with a RFC 1123 date.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <returns>The parsed date.</returns>
        public static DateTime Parse([NotNull] string s)
        {
            if (s.EndsWith("UTC"))
            {
                s = s.Substring(0, s.Length - 3) + "GMT";
            }

            return DateTime.ParseExact(s, "R", CultureInfo.InvariantCulture);
        }

        /// <inheritdoc />
        public DateTime FromElement(XElement element)
        {
            return Parse(element.Value);
        }

        /// <inheritdoc />
        public XElement ToElement(XName name, DateTime value)
        {
            if (value == DateTime.MinValue)
            {
                return new XElement(name);
            }

            return new XElement(name, value.ToString("R"));
        }

        /// <inheritdoc />
        public bool IsValidValue(DateTime value)
        {
            return value != DateTime.MinValue;
        }
    }
}
