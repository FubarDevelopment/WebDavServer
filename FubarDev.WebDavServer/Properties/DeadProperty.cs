using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Properties
{
    public class DeadProperty : IUntypedWriteableProperty, IInitializableProperty
    {
        private readonly IPropertyStore _store;

        private readonly IEntry _entry;

        private XElement _cachedValue;

        public DeadProperty(IPropertyStore store, IEntry entry, XName name)
        {
            Name = name;
            _store = store;
            _entry = entry;
        }

        public DeadProperty(IPropertyStore store, IEntry entry, XElement element)
        {
            _store = store;
            _entry = entry;
            Name = element.Name;
        }

        public XName Name { get; }

        public int Cost => _store.Cost;

        public Task SetXmlValueAsync(XElement element, CancellationToken ct)
        {
            _cachedValue = element;
            return _store.SaveRawAsync(_entry, element, ct);
        }

        public async Task<XElement> GetXmlValueAsync(CancellationToken ct)
        {
            return _cachedValue ?? (_cachedValue = await _store.LoadRawAsync(_entry, Name, ct).ConfigureAwait(false));
        }

        public void Init(XElement initialValue)
        {
            _cachedValue = initialValue;
        }

        protected virtual void UpdateProperty(XElement value)
        {
        }
    }
}
