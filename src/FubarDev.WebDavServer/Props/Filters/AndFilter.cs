// <copyright file="AndFilter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

namespace FubarDev.WebDavServer.Props.Filters
{
    public class AndFilter : TrackingFilter
    {
        private readonly IPropertyFilter[] _filters;

        public AndFilter(params IPropertyFilter[] filters)
        {
            _filters = filters;
        }

        /// <inheritdoc />
        public override bool IsAllowed(IProperty property)
        {
            var isAllowed = true;
            foreach (var filter in _filters)
            {
                // We have to check all filters to ensure that they can return
                // the missing properties properly.
                if (!filter.IsAllowed(property))
                {
                    isAllowed = false;
                }
            }

            return isAllowed;
        }

        /// <inheritdoc />
        public override void NotifyOfSelection(IProperty property)
        {
            base.NotifyOfSelection(property);

            foreach (var filter in _filters)
            {
                filter.NotifyOfSelection(property);
            }
        }

        /// <inheritdoc />
        public override IEnumerable<MissingProperty> GetMissingProperties()
        {
            return _filters.SelectMany(f => f.GetMissingProperties())
                .Where(x => !SelectedProperties.Contains(x.Key))
                .ToLookup(p => p.Key)
                .Select(x => x.First());
        }
    }
}
