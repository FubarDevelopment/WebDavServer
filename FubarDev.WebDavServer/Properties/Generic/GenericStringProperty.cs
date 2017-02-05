// <copyright file="GenericStringProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

using FubarDev.WebDavServer.Properties.Converters;

namespace FubarDev.WebDavServer.Properties.Generic
{
    public class GenericStringProperty : GenericProperty<string>
    {
        public GenericStringProperty(XName name, int cost, GetPropertyValueAsyncDelegate<string> getValueAsyncFunc, SetPropertyValueAsyncDelegate<string> setValueAsyncFunc)
            : base(name, cost, new StringConverter(), getValueAsyncFunc, setValueAsyncFunc)
        {
        }
    }
}
