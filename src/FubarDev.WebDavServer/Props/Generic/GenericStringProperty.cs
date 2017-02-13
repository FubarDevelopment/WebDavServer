// <copyright file="GenericStringProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

using FubarDev.WebDavServer.Props.Converters;

namespace FubarDev.WebDavServer.Props.Generic
{
    public class GenericStringProperty : GenericProperty<string>
    {
        public GenericStringProperty(XName name, int cost, GetPropertyValueAsyncDelegate<string> getValueAsyncFunc, SetPropertyValueAsyncDelegate<string> setValueAsyncFunc, params XName[] alternativeNames)
            : base(name, cost, new StringConverter(), getValueAsyncFunc, setValueAsyncFunc, alternativeNames)
        {
        }
    }
}
