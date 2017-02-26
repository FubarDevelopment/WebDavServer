// <copyright file="IPropertyFilter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FubarDev.WebDavServer.Props.Filters
{
    /// <summary>
    /// An interface to filter the properties to be returned
    /// </summary>
    public interface IPropertyFilter
    {
        /// <summary>
        /// Reset the status of the filter
        /// </summary>
        void Reset();

        /// <summary>
        /// Does this property pass the conditions of this filter?
        /// </summary>
        /// <param name="property">The property to test</param>
        /// <returns><see langref="true"/> when the property passes this filters condition(s)</returns>
        bool IsAllowed(IProperty property);

        /// <summary>
        /// Notify this filter when this property was selected
        /// </summary>
        /// <param name="property">The property that was selected</param>
        void NotifyOfSelection(IProperty property);

        /// <summary>
        /// Gets the properties that weren't selected
        /// </summary>
        /// <returns>The list of properties that weren't selected</returns>
        IEnumerable<MissingProperty> GetMissingProperties();
    }
}
