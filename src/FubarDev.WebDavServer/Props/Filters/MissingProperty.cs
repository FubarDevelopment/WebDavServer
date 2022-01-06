﻿// <copyright file="MissingProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

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
        /// <param name="statusCode">The status code why this property wasn't selected.</param>
        /// <param name="propertyName">The name of the property that wasn't selected.</param>
        public MissingProperty(WebDavStatusCode statusCode, XName propertyName)
        {
            StatusCode = statusCode;
            Key = propertyName;
        }

        /// <summary>
        /// Gets the status code of the reason why this property wasn't selected.
        /// </summary>
        public WebDavStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the name of the property that wasn't selected.
        /// </summary>
        public XName Key { get; }
    }
}
