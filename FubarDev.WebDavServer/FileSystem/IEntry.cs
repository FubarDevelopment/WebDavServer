using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Properties;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface IEntry
    {
        [NotNull]
        string Name { get; }

        [NotNull]
        IFileSystem RootFileSystem { get; }

        [NotNull]
        IFileSystem FileSystem { get; }

        [CanBeNull]
        ICollection Parent { get; }

        [NotNull]
        Uri Path { get; }

        DateTime LastWriteTimeUtc { get; }

        [NotNull]
        IAsyncEnumerable<IUntypedReadableProperty> GetProperties();

        [NotNull]
        [ItemNotNull]
        Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken);
    }
}
