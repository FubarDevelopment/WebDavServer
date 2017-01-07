using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Properties
{
    public abstract class SimpleUntypedProperty : IUntypedReadableProperty, IUntypedWriteableProperty
    {
        protected SimpleUntypedProperty(XName name, int cost)
        {
            Name = name;
            Cost = cost;
        }

        public XName Name { get; }
        public int Cost { get; }
        public abstract Task SetXmlValueAsync(XElement element, CancellationToken ct);
        public abstract Task<XElement> GetXmlValueAsync(CancellationToken ct);
    }
}
