using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Properties.Store;

using JetBrains.Annotations;

using Microsoft.VisualStudio.Threading;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface IFileSystem
    {
        [NotNull]
        AsyncLazy<ICollection> Root { get; }

        [CanBeNull]
        IPropertyStore PropertyStore { get; }

        [NotNull]
        [ItemNotNull]
        Task<SelectionResult> SelectAsync([NotNull] string path, CancellationToken ct);
    }
}
