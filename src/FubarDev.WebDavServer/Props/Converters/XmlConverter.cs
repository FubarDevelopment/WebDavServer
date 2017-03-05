// <copyright file="XmlConverter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace FubarDev.WebDavServer.Props.Converters
{
    /// <summary>
    /// Property converter for an object to be de-/serialized by a <see cref="XmlSerializer"/>
    /// </summary>
    /// <typeparam name="T">The type of the object to be de-/serialized</typeparam>
    public class XmlConverter<T> : IPropertyConverter<T>
        where T : class
    {
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(T));

        /// <inheritdoc />
        public T FromElement(XElement element)
        {
            return (T)_serializer.Deserialize(element.CreateReader());
        }

        /// <inheritdoc />
        public XElement ToElement(XName name, T value)
        {
            var doc = new XDocument();
            using (var writer = doc.CreateWriter())
                _serializer.Serialize(writer, value);
            var element = new XElement(name, doc.Root.Elements().Cast<object>().ToArray());
            return element;
        }

        /// <inheritdoc />
        public bool IsValidValue(T value)
        {
            return value != null;
        }
    }
}
