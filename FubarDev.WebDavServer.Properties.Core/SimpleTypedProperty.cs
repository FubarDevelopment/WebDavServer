using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Properties
{
    public abstract class SimpleTypedProperty<T> : SimpleUntypedProperty, ITypedReadableProperty<T>, ITypedWriteableProperty<T>
    {
        protected SimpleTypedProperty(XName name, int cost)
            : base(name, cost)
        {
        }

        public abstract Task<T> GetValueAsync(CancellationToken ct);
        public abstract Task SetValueAsync(T value, CancellationToken ct);
    }
}