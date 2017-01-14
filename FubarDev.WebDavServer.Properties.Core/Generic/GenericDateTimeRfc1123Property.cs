using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Properties.Converters;

namespace FubarDev.WebDavServer.Properties.Generic
{
    public class GenericDateTimeRfc1123Property : SimpleConvertingProperty<DateTime>
    {
        private readonly GetPropertyValueAsyncDelegate<DateTime> _getValueAsyncFunc;

        private readonly SetPropertyValueAsyncDelegate<DateTime> _setValueAsyncFunc;

        public GenericDateTimeRfc1123Property(XName name, int cost, GetPropertyValueAsyncDelegate<DateTime> getValueAsyncFunc, SetPropertyValueAsyncDelegate<DateTime> setValueAsyncFunc)
            : base(name, cost, new DateTimeRfc1123Converter())
        {
            _getValueAsyncFunc = getValueAsyncFunc;
            _setValueAsyncFunc = setValueAsyncFunc;
        }

        public override Task<DateTime> GetValueAsync(CancellationToken ct)
        {
            return _getValueAsyncFunc(ct);
        }

        public override Task SetValueAsync(DateTime value, CancellationToken ct)
        {
            if (_setValueAsyncFunc == null)
                throw new NotSupportedException();
            return _setValueAsyncFunc(value, ct);
        }
    }
}
