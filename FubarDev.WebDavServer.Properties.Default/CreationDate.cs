using System;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties.Generic;

namespace FubarDev.WebDavServer.Properties.Default
{
    public class CreationDate : GenericDateTimeRfc1123Property
    {
        public CreationDate(GetPropertyValueAsyncDelegate<DateTime> getPropertyValueAsync, SetPropertyValueAsyncDelegate<DateTime> setValueAsyncFunc)
            : base(WebDavXml.Dav + "creationdate", 0, getPropertyValueAsync, setValueAsyncFunc)
        {
        }
    }
}
