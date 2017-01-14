using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Properties.Converters;

namespace FubarDev.WebDavServer.Properties.Generic
{
    public class GenericStringProperty : SimpleConvertingProperty<string>
    {
        private readonly GetPropertyValueAsyncDelegate<string> _getValueAsyncFunc;

        private readonly SetPropertyValueAsyncDelegate<string> _setValueAsyncFunc;

        public GenericStringProperty(XName name, int cost, GetPropertyValueAsyncDelegate<string> getValueAsyncFunc, SetPropertyValueAsyncDelegate<string> setValueAsyncFunc)
            : base(name, cost, new StringConverter())
        {
            _getValueAsyncFunc = getValueAsyncFunc;
            _setValueAsyncFunc = setValueAsyncFunc;
        }

        public override Task<string> GetValueAsync(CancellationToken ct)
        {
            return _getValueAsyncFunc(ct);
        }

        public override Task SetValueAsync(string value, CancellationToken ct)
        {
            if (_setValueAsyncFunc == null)
                throw new NotSupportedException();
            return _setValueAsyncFunc(value, ct);
        }
    }
}
