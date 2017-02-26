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

        private readonly IPropertyFilter _exceptionFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="CostFilter"/> class.
        /// </summary>
        /// <param name="maximumCost">The maximum allowed cost</param>
        public CostFilter(int maximumCost)
            : this(maximumCost, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CostFilter"/> class.
        /// </summary>
        /// <param name="maximumCost">The maximum allowed cost</param>
        /// <param name="exceptionFilter">A filter that allows an exception to this rule</param>
        public CostFilter(int maximumCost, IPropertyFilter exceptionFilter)
        {
            _maximumCost = maximumCost;
            _exceptionFilter = exceptionFilter;
        }

        /// <inheritdoc />
        public void Reset()
        {
        }

        /// <inheritdoc />
        public bool IsAllowed(IProperty property)
        {
            var readableProperty = property as IUntypedReadableProperty;
            if (readableProperty == null)
                throw new InvalidOperationException();
            if (_exceptionFilter != null && _exceptionFilter.IsAllowed(property))
                return true;
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
