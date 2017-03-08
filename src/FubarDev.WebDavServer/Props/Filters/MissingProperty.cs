// <copyright file="MissingProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Props.Filters
{
    /// <summary>
    /// Information about a property that wasn't selected for a PROPFIND
    /// </summary>
    public class MissingProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingProperty"/> class.
        /// </summary>
        /// <param name="statusCode">The status code why this property wasn't selected</param>
        /// <param name="propertyKey">The key of the property that wasn't selected</param>
        public MissingProperty(WebDavStatusCode statusCode, PropertyKey propertyKey)
        {
            StatusCode = statusCode;
            Key = propertyKey;
        }

        /// <summary>
        /// Gets the status code of the reason why this property wasn't selected
        /// </summary>
        public WebDavStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the key of the property that wasn't selected
        /// </summary>
        public PropertyKey Key { get; }
    }
}
