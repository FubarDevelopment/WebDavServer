using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Properties;
using FubarDev.WebDavServer.Properties.Store;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    public class DotNetEntry : IEntry
    {
        public DotNetEntry(DotNetFileSystem fileSystem, FileSystemInfo info, string path)
        {
            Info = info;
            FileSystem = fileSystem;
            Path = path;
        }

        public FileSystemInfo Info { get; }
        public DotNetFileSystem FileSystem { get; }
        public string Name => Info.Name;
        public IFileSystem RootFileSystem => FileSystem;
        public string Path { get; }
        public DateTime LastWriteTimeUtc => Info.LastWriteTimeUtc;

        public virtual IAsyncEnumerable<IProperty> GetProperties()
        {
            var properties = new List<IProperty>()
            {
                this.GetResourceTypeProperty(),
                new DisplayNameProperty(this, FileSystem.PropertyStore),
                new LastModifiedProperty(ct => Task.FromResult(Info.LastWriteTimeUtc), SetLastWriteTimeUtc),
                new CreationDateProperty(ct => Task.FromResult(Info.CreationTimeUtc), SetCreateTimeUtc),
            };

            return new PropertiesEnumerable(this, properties, FileSystem.PropertyStore);
        }

        private Task SetCreateTimeUtc(DateTime value, CancellationToken cancellationToken)
        {
            Info.CreationTimeUtc = value;
            return Task.FromResult(0);
        }

        private Task SetLastWriteTimeUtc(DateTime timestamp, CancellationToken ct)
        {
            Info.LastWriteTimeUtc = timestamp;
            return Task.FromResult(0);
        }
    }
}
