using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Properties;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface IEntry
    {
        string Name { get; }
        IFileSystem RootFileSystem { get; }
        string Path { get; }
        DateTime LastWriteTimeUtc { get; }

        IAsyncEnumerable<IUntypedReadableProperty> GetProperties();
    }
}
