// <copyright file="CostFilter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

namespace FubarDev.WebDavServer.Props.Filters
{
    /// <summary>
    /// Filters a property by its cost to query its value.
    /// </summary>
    public class CostFilter : IPropertyFilter
    {
        private readonly int _maximumCost;

        /// <summary>
        /// Initializes a new instance of the <see cref="CostFilter"/> class.
        /// </summary>
        /// <param name="maximumCost">The maximum allowed cost.</param>
        public CostFilter(int maximumCost)
        {
            _maximumCost = maximumCost;
        }

        /// <inheritdoc />
        public void Reset()
        {
        }

        /// <inheritdoc />
        public bool IsAllowed(IProperty property)
        {
            if (property is not IUntypedReadableProperty readableProperty)
            {
                throw new InvalidOperationException();
            }

            return readableProperty.Cost <= _maximumCost;
        }

        /// <inheritdoc />
        public void NotifyOfSelection(IProperty property)
        {
        }

        /// <inheritdoc />
        public IEnumerable<MissingProperty> GetMissingProperties()
        {
            return Enumerable.Empty<MissingProperty>();
        }
    }
}
