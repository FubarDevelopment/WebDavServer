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
        string Name { get; }
        IFileSystem RootFileSystem { get; }
        IFileSystem FileSystem { get; }
        string Path { get; }
        DateTime LastWriteTimeUtc { get; }

        IAsyncEnumerable<IUntypedReadableProperty> GetProperties();

        [NotNull]
        [ItemNotNull]
        Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken);
    }
}
