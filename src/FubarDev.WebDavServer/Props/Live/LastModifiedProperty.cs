// <copyright file="LastModifiedProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Props.Generic;

namespace FubarDev.WebDavServer.Props.Live
{
    /// <summary>
    /// The <c>getlastmodified</c> property.
    /// </summary>
    public class LastModifiedProperty : GenericDateTimeRfc1123Property, ILiveProperty
    {
        /// <summary>
        /// The XML name of the property.
        /// </summary>
        public static readonly XName PropertyName = WebDavXml.Dav + "getlastmodified";

        /// <summary>
        /// Initializes a new instance of the <see cref="LastModifiedProperty"/> class.
        /// </summary>
        /// <param name="propValue">The initial property value.</param>
        /// <param name="setValueAsyncFunc">The delegate to set the value asynchronously.</param>
        public LastModifiedProperty(DateTime propValue, SetPropertyValueAsyncDelegate<DateTime> setValueAsyncFunc)
            : base(PropertyName, 0, _ => Task.FromResult(propValue), setValueAsyncFunc, WebDavXml.Dav + "lastmodified")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LastModifiedProperty"/> class.
        /// </summary>
        /// <param name="propValue">The initial property value.</param>
        /// <param name="cost">The cost to query the properties value.</param>
        /// <param name="setValueAsyncFunc">The delegate to set the value asynchronously.</param>
        public LastModifiedProperty(DateTime propValue, int cost, SetPropertyValueAsyncDelegate<DateTime> setValueAsyncFunc)
            : base(PropertyName, cost, _ => Task.FromResult(propValue), setValueAsyncFunc, WebDavXml.Dav + "lastmodified")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LastModifiedProperty"/> class.
        /// </summary>
        /// <param name="getValueAsyncFunc">The delegate to get the value asynchronously.</param>
        /// <param name="setValueAsyncFunc">The delegate to set the value asynchronously.</param>
        public LastModifiedProperty(GetPropertyValueAsyncDelegate<DateTime> getValueAsyncFunc, SetPropertyValueAsyncDelegate<DateTime> setValueAsyncFunc)
            : base(PropertyName, 0, getValueAsyncFunc, setValueAsyncFunc, WebDavXml.Dav + "lastmodified")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LastModifiedProperty"/> class.
        /// </summary>
        /// <param name="getValueAsyncFunc">The delegate to get the value asynchronously.</param>
        /// <param name="cost">The cost to query the properties value.</param>
        /// <param name="setValueAsyncFunc">The delegate to set the value asynchronously.</param>
        public LastModifiedProperty(GetPropertyValueAsyncDelegate<DateTime> getValueAsyncFunc, int cost, SetPropertyValueAsyncDelegate<DateTime> setValueAsyncFunc)
            : base(PropertyName, cost, getValueAsyncFunc, setValueAsyncFunc, WebDavXml.Dav + "lastmodified")
        {
        }

        /// <inheritdoc />
        public async Task<bool> IsValidAsync(CancellationToken cancellationToken)
        {
            return Converter.IsValidValue(await GetValueAsync(cancellationToken).ConfigureAwait(false));
        }
    }
}
