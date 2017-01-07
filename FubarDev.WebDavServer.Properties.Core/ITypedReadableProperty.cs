using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Properties
{
    public interface ITypedReadableProperty<T> : IUntypedReadableProperty
    {
        Task<T> GetValueAsync(CancellationToken ct);
    }
}
