// <copyright file="PropertyKeyConverter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Props
{
    internal class PropertyKeyConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var s = (string)value;
            if (s.StartsWith("{"))
                return new PropertyKey(XName.Get(s), null);
            var sepPos = s.IndexOf(':');
            var lang = s.Substring(0, sepPos);
            var name = s.Substring(sepPos + 1);
            return new PropertyKey(XName.Get(name), lang);
        }
    }
}
