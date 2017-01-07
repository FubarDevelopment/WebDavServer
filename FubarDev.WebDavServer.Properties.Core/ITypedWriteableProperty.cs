using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Properties
{
    public interface ITypedWriteableProperty<T> : IUntypedWriteableProperty
    {
        Task SetValueAsync(T value, CancellationToken ct);
    }
}
