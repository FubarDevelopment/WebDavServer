using System;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties.Generic;

namespace FubarDev.WebDavServer.Properties.Default
{
    public class LastModified : GenericDateTimeRfc1123Property
    {
        public LastModified(IEntry entry, SetPropertyValueAsyncDelegate<DateTime> setValueAsyncFunc)
            : base(WebDavXml.Dav + "getlastmodified", 0, ct => Task.FromResult(entry.LastWriteTimeUtc), setValueAsyncFunc)
        {
        }
    }
}
