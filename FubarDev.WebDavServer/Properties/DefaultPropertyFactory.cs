using System.Collections.Generic;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Properties.Store
{
    public class DefaultPropertyFactory : IPropertyFactory
    {
        private readonly IDictionary<XName, CreatePropertyDelegate> _createPropertyDelegates = new Dictionary<XName, CreatePropertyDelegate>()
        {
            [DisplayNameProperty.PropertyName] = (name, cost, entry, store) => new DisplayNameProperty(entry, store),
            [GetETagProperty.PropertyName] = (name, cost, entry, store) => new GetETagProperty(store, entry)
        };

        public IUntypedReadableProperty Create(XName name, IEntry entry, IPropertyStore store)
        {
            CreatePropertyDelegate createFunc;
            if (_createPropertyDelegates.TryGetValue(name, out createFunc))
            {
                return createFunc(name, store.Cost, entry, store);
            }

            return new DeadProperty(store, store.Cost, entry, name);
        }

        public IUntypedReadableProperty Create(XElement initialValue, IEntry entry, IPropertyStore store)
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
    }
}
