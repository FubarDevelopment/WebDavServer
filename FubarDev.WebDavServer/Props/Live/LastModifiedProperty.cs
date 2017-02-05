// <copyright file="LastModifiedProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Props.Generic;

namespace FubarDev.WebDavServer.Props.Live
{
    public class LastModifiedProperty : GenericDateTimeRfc1123Property, ILiveProperty
    {
        public static readonly XName PropertyName = WebDavXml.Dav + "getlastmodified";

        public LastModifiedProperty(GetPropertyValueAsyncDelegate<DateTime> getPropertyValueAsync, SetPropertyValueAsyncDelegate<DateTime> setValueAsyncFunc)
            : base(PropertyName, 0, getPropertyValueAsync, setValueAsyncFunc)
        {
        }
    }
}
