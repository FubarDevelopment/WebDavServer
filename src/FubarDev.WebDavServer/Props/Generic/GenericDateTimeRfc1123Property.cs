// <copyright file="GenericDateTimeRfc1123Property.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Xml.Linq;

using FubarDev.WebDavServer.Props.Converters;

namespace FubarDev.WebDavServer.Props.Generic
{
    /// <summary>
    /// A dead property with a RFC 1123 date
    /// </summary>
    public class GenericDateTimeRfc1123Property : GenericProperty<DateTime>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericDateTimeRfc1123Property"/> class.
        /// </summary>
        /// <param name="name">The property name</param>
        /// <param name="cost">The cost to query the properties value</param>
        /// <param name="getValueAsyncFunc">The function to get the property value</param>
        /// <param name="setValueAsyncFunc">The function to set the property value</param>
        /// <param name="alternativeNames">Alternative property names</param>
        public GenericDateTimeRfc1123Property(XName name, int cost, GetPropertyValueAsyncDelegate<DateTime> getValueAsyncFunc, SetPropertyValueAsyncDelegate<DateTime> setValueAsyncFunc, params XName[] alternativeNames)
            : base(name, null, cost, new DateTimeRfc1123Converter(), getValueAsyncFunc, setValueAsyncFunc, alternativeNames)
        {
        }
    }
}
