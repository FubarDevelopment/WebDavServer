using System;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties.Generic;

namespace FubarDev.WebDavServer.Properties
{
    public class CreationDate : GenericDateTimeRfc1123Property
    {
        public static readonly XName PropertyName = WebDavXml.Dav + "creationdate";

        public CreationDate(GetPropertyValueAsyncDelegate<DateTime> getPropertyValueAsync, SetPropertyValueAsyncDelegate<DateTime> setValueAsyncFunc)
            : base(PropertyName, 0, getPropertyValueAsync, setValueAsyncFunc)
        {
        }
    }
}
