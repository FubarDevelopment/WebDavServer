// <copyright file="GenericStringProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

using FubarDev.WebDavServer.Props.Converters;

namespace FubarDev.WebDavServer.Props.Generic
{
    /// <summary>
    /// A dead property with a string value.
    /// </summary>
    public class GenericStringProperty : GenericProperty<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericStringProperty"/> class.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="language">The language for the property value.</param>
        /// <param name="cost">The cost to query the properties value.</param>
        /// <param name="getValueAsyncFunc">The function to get the property value.</param>
        /// <param name="setValueAsyncFunc">The function to set the property value.</param>
        /// <param name="alternativeNames">Alternative property names.</param>
        public GenericStringProperty(XName name, string? language, int cost, GetPropertyValueAsyncDelegate<string> getValueAsyncFunc, SetPropertyValueAsyncDelegate<string> setValueAsyncFunc, params XName[] alternativeNames)
            : base(name, language, cost, new StringConverter(), getValueAsyncFunc, setValueAsyncFunc, alternativeNames)
        {
        }
    }
}
