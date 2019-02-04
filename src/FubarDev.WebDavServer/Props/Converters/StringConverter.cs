// <copyright file="StringConverter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

namespace FubarDev.WebDavServer.Props.Converters
{
    /// <summary>
    /// Property converter for a <see langword="string"/>.
    /// </summary>
    public class StringConverter : IPropertyConverter<string>
    {
        /// <inheritdoc />
        public string FromElement(XElement element)
        {
            return element.Value;
        }

        /// <inheritdoc />
        public XElement ToElement(XName name, string value)
        {
            return new XElement(name, value);
        }

        /// <inheritdoc />
        public bool IsValidValue(string value)
        {
            return value != null;
        }
    }
}
