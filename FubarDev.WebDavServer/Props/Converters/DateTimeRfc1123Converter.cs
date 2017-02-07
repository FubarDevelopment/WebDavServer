// <copyright file="DateTimeRfc1123Converter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Globalization;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Props.Converters
{
    public class DateTimeRfc1123Converter : IPropertyConverter<DateTime>
    {
        public DateTime FromElement(XElement element)
        {
            var v = element.Value;
            if (v.EndsWith("UTC"))
                v = v.Substring(0, v.Length - 3) + "GMT";
            return DateTime.ParseExact(v, "R", CultureInfo.InvariantCulture);
        }

        public XElement ToElement(XName name, DateTime value)
        {
            return new XElement(name, value.ToString("R"));
        }
    }
}
