using System;
using System.Threading;
using System.Threading.Tasks;
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
