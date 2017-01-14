using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties.Generic;

namespace FubarDev.WebDavServer.Properties.Store
{
    public class DefaultPropertyFactory : IPropertyFactory
    {
        private readonly IDictionary<XName, CreatePropertyDelegate> _createPropertyDelegates = new Dictionary<XName, CreatePropertyDelegate>()
        {
            [DisplayName.PropertyName] = (name, cost, entry, store) => new DisplayName(entry, store, cost)
        };

        public IUntypedWriteableProperty Create(XName name, IEntry entry, IPropertyStore store)
        {
            CreatePropertyDelegate createFunc;
            if (_createPropertyDelegates.TryGetValue(name, out createFunc))
            {
                return createFunc(name, store.Cost, entry, store);
            }

            return new DeadProperty(store, store.Cost, entry, name);
        }

        public IUntypedWriteableProperty Create(XElement initialValue, IEntry entry, IPropertyStore store)
        {
            CreatePropertyDelegate createFunc;
            if (_createPropertyDelegates.TryGetValue(initialValue.Name, out createFunc))
            {
                var prop = createFunc(initialValue.Name, store.Cost, entry, store);
                ((IInitializableProperty)prop).Init(initialValue);
                return prop;
            }

            return new DeadProperty(store, store.Cost, entry, initialValue);
        }

        private class DisplayName : GenericStringProperty, IInitializableProperty
        {
            public static readonly XName PropertyName = WebDavXml.Dav + "displayname";

            private readonly IEntry _entry;

            private readonly IPropertyStore _store;

            private string _value;

            public DisplayName(IEntry entry, IPropertyStore store, int cost)
                : base(PropertyName, cost, null, null)
            {
                _entry = entry;
                _store = store;
            }

            public override async Task<string> GetValueAsync(CancellationToken ct)
            {
                if (_value != null)
                    return _value;

                var displayName = await _store.LoadRawAsync(_entry, Name, ct).ConfigureAwait(false);
                if (displayName != null)
                {
                    return displayName.Value;
                }

                var newName = _value = Path.GetFileNameWithoutExtension(_entry.Name);
                await SetValueAsync(newName, ct).ConfigureAwait(false);
                return newName;
            }

            public override Task SetValueAsync(string value, CancellationToken ct)
            {
                _value = value;
                return _store.SaveRawAsync(_entry, Converter.ToElement(Name, value), ct);
            }

            public void Init(XElement initialValue)
            {
                _value = Converter.FromElement(initialValue);
            }
        }
    }
}
