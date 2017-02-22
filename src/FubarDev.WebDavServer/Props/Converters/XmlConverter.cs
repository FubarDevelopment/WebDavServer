// <copyright file="XmlConverter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace FubarDev.WebDavServer.Props.Converters
{
    public class XmlConverter<T> : IPropertyConverter<T>
    {
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(T));

        public T FromElement(XElement element)
        {
            return (T)_serializer.Deserialize(element.CreateReader());
        }

        public XElement ToElement(XName name, T value)
        {
            var output = new StringWriter();
            _serializer.Serialize(output, value);
            var doc = XDocument.Parse(output.ToString());
            return doc.Root;
        }
    }
}
