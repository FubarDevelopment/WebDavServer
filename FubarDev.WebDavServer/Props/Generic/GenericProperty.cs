// <copyright file="GenericProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Props.Converters;

namespace FubarDev.WebDavServer.Props.Generic
{
    public class GenericProperty<T> : SimpleConvertingProperty<T>
    {
        private readonly GetPropertyValueAsyncDelegate<T> _getValueAsyncFunc;

        private readonly SetPropertyValueAsyncDelegate<T> _setValueAsyncFunc;

        public GenericProperty(XName name, int cost, IPropertyConverter<T> converter, GetPropertyValueAsyncDelegate<T> getValueAsyncFunc, SetPropertyValueAsyncDelegate<T> setValueAsyncFunc)
            : base(name, cost, converter)
        {
            _getValueAsyncFunc = getValueAsyncFunc;
            _setValueAsyncFunc = setValueAsyncFunc;
        }

        public override Task<T> GetValueAsync(CancellationToken ct)
        {
            return _getValueAsyncFunc(ct);
        }

        public override Task SetValueAsync(T value, CancellationToken ct)
        {
            if (_setValueAsyncFunc == null)
                throw new NotSupportedException();
            return _setValueAsyncFunc(value, ct);
        }
    }
}
