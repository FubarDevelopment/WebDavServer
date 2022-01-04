// <copyright file="OrFilter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Props.Filters
{
    /// <summary>
    /// This filter matches if any of the child filters match.
    /// </summary>
    public class OrFilter : IPropertyFilter
    {
        private readonly Dictionary<XName, IPropertyFilter> _selectedProperties = new();
        private readonly IPropertyFilter[] _filters;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrFilter"/> class.
        /// </summary>
        /// <param name="filters">The child filters.</param>
        public OrFilter(params IPropertyFilter[] filters)
        {
            _filters = filters;
        }

        /// <inheritdoc />
        public void Reset()
        {
            _selectedProperties.Clear();

            foreach (var filter in _filters)
            {
                filter.Reset();
            }
        }

        /// <inheritdoc />
        public bool IsAllowed(IProperty property)
        {
            if (_filters.Length == 0)
            {
                return true;
            }

            // Property requested twice?
            if (_selectedProperties.ContainsKey(property.Name))
            {
                return true;
            }

            var isAllowed = false;
            foreach (var filter in _filters)
            {
                // We have to check all filters to ensure that they can return
                // the missing properties properly.
                if (filter.IsAllowed(property))
                {
                    _selectedProperties[property.Name] = filter;
                    isAllowed = true;
                }
            }

            return isAllowed;
        }

        /// <inheritdoc />
        public void NotifyOfSelection(IProperty property)
        {
            if (_filters.Length == 0)
            {
                return;
            }

            _selectedProperties[property.Name].NotifyOfSelection(property);
        }

        /// <inheritdoc />
        public IEnumerable<MissingProperty> GetMissingProperties()
        {
            return _filters.SelectMany(f => f.GetMissingProperties())
                .Where(x => !_selectedProperties.ContainsKey(x.Key))
                .ToLookup(p => p.Key)
                .Select(x => x.First());
        }
    }
}
