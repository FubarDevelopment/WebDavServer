// <copyright file="XmlConverter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Linq;
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
            var doc = new XDocument();
            using (var writer = doc.CreateWriter())
                _serializer.Serialize(writer, value);
            var element = new XElement(name, doc.Root.Elements().Cast<object>().ToArray());
            return element;
        }
    }
}
