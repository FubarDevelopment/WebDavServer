// <copyright file="StringConverter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

namespace FubarDev.WebDavServer.Properties.Converters
{
    public class StringConverter : IPropertyConverter<string>
    {
        public string FromElement(XElement element)
        {
            return element.Value;
        }

        public XElement ToElement(XName name, string value)
        {
            return new XElement(name, value);
        }
    }
}
