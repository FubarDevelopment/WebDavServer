// <copyright file="CostFilter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

namespace FubarDev.WebDavServer.Properties.Filters
{
    public class CostFilter : IPropertyFilter
    {
        private readonly int _maximumCost;

        private readonly IPropertyFilter _exceptionFilter;

        public CostFilter(int maximumCost)
            : this(maximumCost, null)
        {
        }

        public CostFilter(int maximumCost, IPropertyFilter exceptionFilter)
        {
            _maximumCost = maximumCost;
            _exceptionFilter = exceptionFilter;
        }

        public void Reset()
        {
        }

        public bool IsAllowed(IProperty property)
        {
            var readableProperty = property as IUntypedReadableProperty;
            if (readableProperty == null)
                throw new InvalidOperationException();
            if (_exceptionFilter != null && _exceptionFilter.IsAllowed(property))
                return true;
            return readableProperty.Cost <= _maximumCost;
        }

        public void NotifyOfSelection(IProperty property)
        {
        }

        public IEnumerable<MissingProperty> GetMissingProperties()
        {
            return Enumerable.Empty<MissingProperty>();
        }
    }
}
