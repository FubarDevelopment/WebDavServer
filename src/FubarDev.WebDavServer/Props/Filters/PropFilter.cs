// <copyright file="PropFilter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Props.Filters
{
    /// <summary>
    /// Filters the allowed properties by name.
    /// </summary>
    public class PropFilter : TrackingFilter
    {
        private readonly ImmutableHashSet<XName> _requestedProperties;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropFilter"/> class.
        /// </summary>
        /// <param name="prop">The <see cref="Models.prop"/> element containing the property names.</param>
        public PropFilter(Models.prop prop)
        {
            _requestedProperties = prop.Any.Select(x => x.Name).ToImmutableHashSet();
        }

        /// <inheritdoc />
        public override bool IsAllowed(IProperty property)
        {
            return _requestedProperties.Contains(property.Name);
        }

        /// <inheritdoc />
        public override IEnumerable<MissingProperty> GetMissingProperties()
        {
            var missingProps = _requestedProperties.Except(SelectedProperties);
            return missingProps.Select(x => new MissingProperty(WebDavStatusCode.NotFound, x));
        }
    }
}
