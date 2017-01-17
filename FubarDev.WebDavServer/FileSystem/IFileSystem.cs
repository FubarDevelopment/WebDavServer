using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Properties;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface IFileSystem
    {
        bool AllowInfiniteDepth { get; }

        [NotNull]
        AsyncLazy<ICollection> Root { get; }

        [CanBeNull]
        IPropertyStore PropertyStore { get; }

        [NotNull]
        [ItemNotNull]
        Task<SelectionResult> SelectAsync([NotNull] string path, CancellationToken ct);
    }
}
