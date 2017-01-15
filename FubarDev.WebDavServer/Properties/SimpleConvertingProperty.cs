using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Properties
{
    public abstract class SimpleConvertingProperty<T> : SimpleTypedProperty<T>
    {
        protected SimpleConvertingProperty([NotNull] XName name, int cost, [NotNull] IPropertyConverter<T> converter)
            : base(name, cost)
        {
            Converter = converter;
        }

        [NotNull]
        protected IPropertyConverter<T> Converter { get; }

        public override async Task<XElement> GetXmlValueAsync(CancellationToken ct)
        {
            var result = await GetValueAsync(ct).ConfigureAwait(false);
            return Converter.ToElement(Name, result);
        }

        public override Task SetXmlValueAsync(XElement element, CancellationToken ct)
        {
            return SetValueAsync(Converter.FromElement(element), ct);
        }
    }
}
