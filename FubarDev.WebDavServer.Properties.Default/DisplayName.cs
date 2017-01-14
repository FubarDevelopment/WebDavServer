using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties.Converters;

namespace FubarDev.WebDavServer.Properties.Default
{
    public class DisplayName : SimpleConvertingProperty<string>
    {
        private readonly IEntry _entry;

        private readonly bool _hideExtension;

        private readonly Func<string, CancellationToken, Task> _setValueAsyncFunc;

        public DisplayName(IEntry entry, bool hideExtension)
            : this(entry, hideExtension, null)
        {
        }

        public DisplayName(IEntry entry, bool hideExtension, Func<string, CancellationToken, Task> setValueAsyncFunc)
            : base(WebDavXml.Dav + "displayname", 0, new StringConverter())
        {
            _entry = entry;
            _hideExtension = hideExtension;
            _setValueAsyncFunc = setValueAsyncFunc;
        }

        public override Task<string> GetValueAsync(CancellationToken ct)
        {
            var result = _entry.Name;
            if (_hideExtension)
                result = Path.GetFileNameWithoutExtension(result);
            return Task.FromResult(result);
        }

        public override Task SetValueAsync(string value, CancellationToken ct)
        {
            if (_setValueAsyncFunc == null)
                throw new NotSupportedException();

            if (_hideExtension)
            {
                var oldExtension = Path.GetExtension(_entry.Name);
                value = value + oldExtension;
            }

            return _setValueAsyncFunc(value, ct);
        }
    }
}
