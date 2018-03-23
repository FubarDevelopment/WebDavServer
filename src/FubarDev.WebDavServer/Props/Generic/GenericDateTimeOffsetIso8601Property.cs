// <copyright file="GenericDateTimeOffsetIso8601Property.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Xml.Linq;

using FubarDev.WebDavServer.Props.Converters;

namespace FubarDev.WebDavServer.Props.Generic
{
    /// <summary>
    /// A dead property with a ISO 8601 date
    /// </summary>
    public class GenericDateTimeOffsetIso8601Property : GenericProperty<DateTimeOffset>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericDateTimeOffsetIso8601Property"/> class.
        /// </summary>
        /// <param name="name">The property name</param>
        /// <param name="cost">The cost to query the properties value</param>
        /// <param name="getValueAsyncFunc">The function to get the property value</param>
        /// <param name="setValueAsyncFunc">The function to set the property value</param>
        /// <param name="alternativeNames">Alternative property names</param>
        public GenericDateTimeOffsetIso8601Property(XName name, int cost, GetPropertyValueAsyncDelegate<DateTimeOffset> getValueAsyncFunc, SetPropertyValueAsyncDelegate<DateTimeOffset> setValueAsyncFunc, params XName[] alternativeNames)
            : base(name, null, cost, new DateTimeOffsetIso8601Converter(), getValueAsyncFunc, setValueAsyncFunc, alternativeNames)
        {
        }
    }
}
