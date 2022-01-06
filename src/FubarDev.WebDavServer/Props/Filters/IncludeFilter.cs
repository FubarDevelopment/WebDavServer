// <copyright file="IncludeFilter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;

using FubarDev.WebDavServer.Models;

namespace FubarDev.WebDavServer.Props.Filters
{
    /// <summary>
    /// Filter for properties to include.
    /// </summary>
    public class IncludeFilter : TrackingFilter
    {
        private readonly ImmutableHashSet<XName> _requestedProperties;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncludeFilter"/> class.
        /// </summary>
        /// <param name="include">The parameters to <c>include</c>.</param>
        public IncludeFilter(include? include)
        {
            _requestedProperties =
                include?.Any.Select(x => x.Name).ToImmutableHashSet()
                ?? ImmutableHashSet<XName>.Empty;
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
