using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties.Converters;
using FubarDev.WebDavServer.Properties.Store;

namespace FubarDev.WebDavServer.Properties.Dead
{
    public class GetETagProperty : ITypedReadableProperty<EntityTag>, IDeadProperty
    {
        public static readonly XName PropertyName = WebDavXml.Dav + "getetag";

        private readonly IPropertyStore _propertyStore;

        private readonly IEntry _entry;

        private XElement _element;

        public GetETagProperty(IPropertyStore propertyStore, IEntry entry, int? cost = null)
        {
            _propertyStore = propertyStore;
            _entry = entry;
            Name = PropertyName;
            Cost = cost ?? _propertyStore.Cost;
        }

        public XName Name { get; }

        public int Cost { get; }

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
                    var etag = await _propertyStore.GetETagAsync(document, ct).ConfigureAwait(false);
                    _element = Converter.ToElement(Name, etag);
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
