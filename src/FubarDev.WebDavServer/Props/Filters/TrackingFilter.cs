// <copyright file="TrackingFilter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Props.Filters
{
    public abstract class TrackingFilter : IPropertyFilter
    {
        private ImmutableHashSet<XName> _selectedProperties = ImmutableHashSet<XName>.Empty;

        /// <summary>
        /// Gets the selected properties.
        /// </summary>
        protected ISet<XName> SelectedProperties => _selectedProperties;

        /// <inheritdoc />
        public void Reset()
        {
            _selectedProperties = ImmutableHashSet<XName>.Empty;
        }

        /// <inheritdoc />
        public abstract bool IsAllowed(IProperty property);

        /// <inheritdoc />
        public virtual void NotifyOfSelection(IProperty property)
        {
            _selectedProperties = _selectedProperties.Add(property.Name);
        }

        /// <inheritdoc />
        public abstract IEnumerable<MissingProperty> GetMissingProperties();
    }
}
