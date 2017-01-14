using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties.Converters;

namespace FubarDev.WebDavServer.Properties.Default
{
    public class ContentLength : ITypedReadableProperty<long>
    {
        private readonly GetPropertyValueAsyncDelegate<long> _getPropertyValueAsync;

        private static readonly LongConverter _converter = new LongConverter();

        public ContentLength(GetPropertyValueAsyncDelegate<long> getPropertyValueAsync)
        {
            Cost = 0;
            Name = WebDavXml.Dav + "getcontentlength";
            _getPropertyValueAsync = getPropertyValueAsync;
        }

        public Task<long> GetValueAsync(CancellationToken ct)
        {
            return _getPropertyValueAsync(ct);
        }

        public XName Name { get; }

        public int Cost { get; }

        public async Task<XElement> GetXmlValueAsync(CancellationToken ct)
        {
            var length = await GetValueAsync(ct).ConfigureAwait(false);
            return _converter.ToElement(Name, length);
        }
    }
}
