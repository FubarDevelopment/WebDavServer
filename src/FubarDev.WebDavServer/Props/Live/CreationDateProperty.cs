// <copyright file="CreationDateProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Props.Generic;

namespace FubarDev.WebDavServer.Props.Live
{
    public class CreationDateProperty : GenericDateTimeRfc1123Property, ILiveProperty
    {
        public static readonly XName PropertyName = WebDavXml.Dav + "creationdate";

        public CreationDateProperty(DateTime propValue, SetPropertyValueAsyncDelegate<DateTime> setValueAsyncFunc)
            : base(PropertyName, 0, ct => Task.FromResult(propValue), setValueAsyncFunc)
        {
        }
    }
}
