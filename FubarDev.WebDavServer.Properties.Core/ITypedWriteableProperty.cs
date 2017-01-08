using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Properties
{
    public interface ITypedWriteableProperty<T> : IUntypedWriteableProperty
    {
        [NotNull]
        Task SetValueAsync([NotNull] T value, CancellationToken ct);
    }
}
