using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FubarDev.WebDavServer.Properties;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    public class DotNetEntry : IEntry
    {
        private readonly Lazy<IEnumerable<IProperty>> _properties;

        public DotNetEntry(DotNetFileSystem fileSystem, FileSystemInfo info, string path)
        {
            _properties = new Lazy<IEnumerable<IProperty>>(CreateProperties);
            Info = info;
            FileSystem = fileSystem;
            Path = path;
        }

        public FileSystemInfo Info { get; }
        public DotNetFileSystem FileSystem { get; }

        public string Name => Info.Name;
        public IFileSystem RootFileSystem => FileSystem;
        public string Path { get; }
        public IEnumerable<IProperty> Properties => _properties.Value;

        private IEnumerable<IProperty> CreateProperties()
        {
            return Enumerable.Empty<IProperty>();
        }
    }
}
