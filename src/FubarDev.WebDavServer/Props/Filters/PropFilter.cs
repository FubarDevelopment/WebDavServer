// <copyright file="PropFilter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Props.Filters
{
    /// <summary>
    /// Filters the allowed properties by name
    /// </summary>
    public class PropFilter : IPropertyFilter
    {
        private readonly ISet<PropertyKey> _selectedProperties;

        private readonly HashSet<PropertyKey> _requestedProperties;

        private readonly IReadOnlyDictionary<XName, IReadOnlyList<string>> _languagesByNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropFilter"/> class.
        /// </summary>
        /// <param name="prop">The <see cref="prop"/> element containing the property names</param>
        public PropFilter(prop prop)
        {
            _requestedProperties = new HashSet<PropertyKey>(prop.Any.Select(x => CreateKey(x, prop.Language)));
            _languagesByNames = _requestedProperties
                .GroupBy(x => x.Name)
                .ToDictionary(x => x.Key, x => (IReadOnlyList<string>)x.Select(y => y.Language).ToList());
            _selectedProperties = new HashSet<PropertyKey>();
        }

        /// <inheritdoc />
        public void Reset()
        {
            _selectedProperties.Clear();
        }

        /// <inheritdoc />
        public bool IsAllowed(IProperty property)
        {
            if (_requestedProperties.Contains(new PropertyKey(property)))
                return true;
            if (!_languagesByNames.TryGetValue(property.Name, out var names))
                return false;
            if (names.Count == 1 && names[0] == PropertyKey.NoLanguage)
                return true;
            return false;
        }

        /// <inheritdoc />
        public void NotifyOfSelection(IProperty property)
        {
            _selectedProperties.Add(new PropertyKey(property));
        }

        /// <inheritdoc />
        public IEnumerable<MissingProperty> GetMissingProperties()
        {
            var missingProps = new HashSet<PropertyKey>(_requestedProperties);
            missingProps.ExceptWith(_selectedProperties);
            return missingProps.Select(x => new MissingProperty(WebDavStatusCode.NotFound, x));
        }

        private PropertyKey CreateKey(XElement element, string defaultLanguage)
        {
            var elementLanguage = element.Attribute(XNamespace.Xml + "lang")?.Value ?? defaultLanguage ?? PropertyKey.NoLanguage;
            return new PropertyKey(element.Name, elementLanguage);
        }
    }
}
