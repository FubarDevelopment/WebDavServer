using System.Collections.Generic;

using FubarDev.WebDavServer.Properties;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface IEntry
    {
        string Name { get; }
        IFileSystem RootFileSystem { get; }
        string Path { get; }
        IEnumerable<IProperty> Properties { get; }
    }
}
