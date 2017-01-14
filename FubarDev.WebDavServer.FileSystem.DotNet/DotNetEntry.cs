using System;
using System.Collections.Generic;
using System.IO;

using FubarDev.WebDavServer.Properties;
using FubarDev.WebDavServer.Properties.Default;

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

        protected virtual IEnumerable<IProperty> CreateProperties()
        {
            yield return this.GetResourceTypeProperty();

            // Display name is should not move the file or collection, but it also should not be protected.
            // The usual file systems can only handle this by extended attributes which aren't available
            // through the standard .NET IO API. For this to work, we have to implement an alternate
            // property store which isn't there yet.
            yield return new DisplayName(this, FileSystem.Options.HideExtensionsForDisplayName);
        }
    }
}
