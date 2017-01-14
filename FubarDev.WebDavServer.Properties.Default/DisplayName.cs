using System;

using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties.Generic;

namespace FubarDev.WebDavServer.Properties.Default
{
    public class DisplayName : GenericStringProperty
    {
        public DisplayName(GetPropertyValueAsyncDelegate<string> getValueAsyncFunc, SetPropertyValueAsyncDelegate<string> setValueAsyncFunc)
            : base(WebDavXml.Dav + "displayname", 0, getValueAsyncFunc, setValueAsyncFunc)
        {
        }
    }
}
