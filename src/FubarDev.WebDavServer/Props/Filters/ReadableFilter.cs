// <copyright file="ReadableFilter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

namespace FubarDev.WebDavServer.Props.Filters
{
    /// <summary>
    /// Implementation of a property filter that allows only readable properties
    /// </summary>
    public class ReadableFilter : IPropertyFilter
    {
        /// <inheritdoc />
        public void Reset()
        {
        }

        /// <inheritdoc />
        public bool IsAllowed(IProperty property)
        {
            return property is IUntypedReadableProperty;
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
