using System;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties.Generic;

namespace FubarDev.WebDavServer.Properties
{
    public class CreationDateProperty : GenericDateTimeRfc1123Property
    {
        public static readonly XName PropertyName = WebDavXml.Dav + "creationdate";

        public CreationDateProperty(GetPropertyValueAsyncDelegate<DateTime> getPropertyValueAsync, SetPropertyValueAsyncDelegate<DateTime> setValueAsyncFunc)
            : base(PropertyName, 0, getPropertyValueAsync, setValueAsyncFunc)
        {
        }
    }
}
