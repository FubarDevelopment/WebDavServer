using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties.Generic;

namespace FubarDev.WebDavServer.Properties
{
    public class DisplayNameProperty : GenericStringProperty, IInitializableProperty
    {
        public static readonly XName PropertyName = WebDavXml.Dav + "displayname";

        private readonly IEntry _entry;

        private readonly IPropertyStore _store;

        private string _value;

        public DisplayNameProperty(IEntry entry, IPropertyStore store)
            : base(PropertyName, store.Cost, null, null)
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
