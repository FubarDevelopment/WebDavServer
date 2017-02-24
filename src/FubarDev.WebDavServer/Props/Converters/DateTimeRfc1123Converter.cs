// <copyright file="DateTimeRfc1123Converter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Globalization;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props.Converters
{
    public class DateTimeRfc1123Converter : IPropertyConverter<DateTime>
    {
        public static DateTime Parse([NotNull] string s)
        {
            if (s.EndsWith("UTC"))
                s = s.Substring(0, s.Length - 3) + "GMT";
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
            return new XElement(name, value.ToString("R"));
        }
    }
}
