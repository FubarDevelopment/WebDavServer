using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties.Converters;

namespace FubarDev.WebDavServer.Properties
{
    public class GetETagProperty : ITypedReadableProperty<EntityTag>, IInitializableProperty
    {
        public static readonly XName PropertyName = WebDavXml.Dav + "getetag";

        private readonly IPropertyStore _propertyStore;

        private readonly IEntry _entry;

        private XElement _element;

        public GetETagProperty(IPropertyStore propertyStore, IEntry entry)
        {
            _propertyStore = propertyStore;
            _entry = entry;
            Name = PropertyName;
        }

        public XName Name { get; }

        public int Cost => _propertyStore.Cost;

        public IPropertyConverter<EntityTag> Converter { get; } = new EntityTagConverter();

        public async Task<XElement> GetXmlValueAsync(CancellationToken ct)
        {
            if (_element == null)
            {
                var document = _entry as IDocument;
                if (document == null)
                {
                    _element = Converter.ToElement(Name, new EntityTag());
                }
                else
                {
                    _element = await _propertyStore.LoadRawAsync(document, Name, ct).ConfigureAwait(false);
                }
            }

            return _element;
        }

        public void Init(XElement initialValue)
        {
            _element = initialValue;
        }

        public async Task<EntityTag> GetValueAsync(CancellationToken ct)
        {
            return Converter.FromElement(await GetXmlValueAsync(ct).ConfigureAwait(false));
        }
    }
}
