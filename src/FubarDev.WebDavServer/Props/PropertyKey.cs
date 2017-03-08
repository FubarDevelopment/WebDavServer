// <copyright file="PropertyKey.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props
{
    /// <summary>
    /// The key for a property consisting of <see cref="Name"/> and <see cref="Language"/>
    /// </summary>
    [TypeConverter(typeof(PropertyKeyConverter))]
    public struct PropertyKey : IEquatable<PropertyKey>
    {
        /// <summary>
        /// The string that identifies that no <c>xml:lang</c> was defined for a property
        /// </summary>
        public const string NoLanguage = "*";

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyKey"/> struct.
        /// </summary>
        /// <param name="name">The property name</param>
        /// <param name="language">The property value language</param>
        public PropertyKey([NotNull] XName name, [CanBeNull] string language)
        {
            Name = name;
            Language = string.IsNullOrEmpty(language) ? NoLanguage : language;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyKey"/> struct.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> to create a key for</param>
        public PropertyKey(XElement element)
            : this(element.Name, element.Attribute(XNamespace.Xml + "lang")?.Value ?? NoLanguage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyKey"/> struct.
        /// </summary>
        /// <param name="property">The <see cref="IProperty"/> to create a key for</param>
        public PropertyKey(IProperty property)
            : this(property.Name, property.Language)
        {
        }

        /// <summary>
        /// Gets the property name
        /// </summary>
        [NotNull]
        public XName Name { get; }

        /// <summary>
        /// Gets the property language
        /// </summary>
        [NotNull]
        public string Language { get; }

        /// <summary>
        /// Gets the property element language
        /// </summary>
        [CanBeNull]
        public string ElementLanguage => Language == NoLanguage ? null : Language;

        /// <summary>
        /// Creates a new empty element with the <c>xml:lang</c> attribute set to the value of <see cref="ElementLanguage"/>.
        /// </summary>
        /// <returns>The new element</returns>
        public XElement CreateEmptyElement()
        {
            var element = new XElement(Name);
            if (Language != NoLanguage)
                element.Add(new XAttribute(XNamespace.Xml + "lang", Language));
            return element;
        }

        /// <inheritdoc />
        public bool Equals(PropertyKey other)
        {
            return PropertyKeyComparer.Default.Equals(this, other);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (obj.GetType() != GetType())
                return false;
            return Equals((PropertyKey)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return PropertyKeyComparer.Default.GetHashCode(this);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Language}:{Name}";
        }
    }
}
