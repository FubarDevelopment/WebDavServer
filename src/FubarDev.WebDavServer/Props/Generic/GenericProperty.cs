// <copyright file="GenericProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Props.Converters;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props.Generic
{
    /// <summary>
    /// A generic typed property.
    /// </summary>
    /// <typeparam name="T">The underlying property value type.</typeparam>
    public class GenericProperty<T> : SimpleConvertingProperty<T>
    {
        private readonly GetPropertyValueAsyncDelegate<T> _getValueAsyncFunc;

        private readonly SetPropertyValueAsyncDelegate<T> _setValueAsyncFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericProperty{T}"/> class.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="language">The language for the property value.</param>
        /// <param name="cost">The cost to query the properties value.</param>
        /// <param name="converter">The converter to convert to/from the underlying property value.</param>
        /// <param name="getValueAsyncFunc">The function to get the property value.</param>
        /// <param name="setValueAsyncFunc">The function to set the property value.</param>
        /// <param name="alternativeNames">Alternative property names.</param>
        public GenericProperty([NotNull] XName name, [CanBeNull] string language, int cost, [NotNull] IPropertyConverter<T> converter, GetPropertyValueAsyncDelegate<T> getValueAsyncFunc, SetPropertyValueAsyncDelegate<T> setValueAsyncFunc, params XName[] alternativeNames)
            : base(name, language, cost, converter, alternativeNames)
        {
            _getValueAsyncFunc = getValueAsyncFunc;
            _setValueAsyncFunc = setValueAsyncFunc;
        }

        /// <inheritdoc />
        public override Task<T> GetValueAsync(CancellationToken ct)
        {
            return _getValueAsyncFunc(ct);
        }

        /// <inheritdoc />
        public override Task SetValueAsync(T value, CancellationToken ct)
        {
            if (_setValueAsyncFunc == null)
                throw new NotSupportedException();
            return _setValueAsyncFunc(value, ct);
        }
    }
}
