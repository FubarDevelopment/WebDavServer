// <copyright file="LongConverter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Props.Converters
{
    /// <summary>
    /// Converter between <see cref="XElement"/> and <see langword="long"/>
    /// </summary>
    public class LongConverter : IPropertyConverter<long>
    {
        /// <inheritdoc />
        public long FromElement(XElement element)
        {
            return XmlConvert.ToInt64(element.Value);
        }

        /// <inheritdoc />
        public XElement ToElement(XName name, long value)
        {
            return new XElement(name, XmlConvert.ToString(value));
        }
    }
}
