// <copyright file="GenericDateTimeRfc1123Property.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Xml.Linq;

using FubarDev.WebDavServer.Properties.Converters;

namespace FubarDev.WebDavServer.Properties.Generic
{
    public class GenericDateTimeRfc1123Property : GenericProperty<DateTime>
    {
        public GenericDateTimeRfc1123Property(XName name, int cost, GetPropertyValueAsyncDelegate<DateTime> getValueAsyncFunc, SetPropertyValueAsyncDelegate<DateTime> setValueAsyncFunc)
            : base(name, cost, new DateTimeRfc1123Converter(), getValueAsyncFunc, setValueAsyncFunc)
        {
        }
    }
}
