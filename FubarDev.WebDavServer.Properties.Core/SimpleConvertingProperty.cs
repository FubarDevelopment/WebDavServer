using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Properties
{
    public abstract class SimpleConvertingProperty<T> : SimpleTypedProperty<T>
    {
        protected SimpleConvertingProperty(XName name, int cost, IPropertyConverter<T> converter) 
            : base(name, cost)
        {
            Converter = converter;
        }

        protected IPropertyConverter<T> Converter { get; }

        public override async Task<XElement> GetXmlValueAsync(CancellationToken ct)
        {
            var result = await GetValueAsync(ct);
            return Converter.ToElement(result);
        }

        public override Task SetXmlValueAsync(XElement element, CancellationToken ct)
        {
            return SetValueAsync(Converter.FromElement(element), ct);
        }
    }
}
