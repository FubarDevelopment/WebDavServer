using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Properties;
using FubarDev.WebDavServer.Properties.Dead;
using FubarDev.WebDavServer.Properties.Live;

namespace FubarDev.WebDavServer.FileSystem.InMemory
{
    public abstract class InMemoryEntry : IEntry
    {
        private readonly InMemoryDirectory _parent;

        protected InMemoryEntry(IFileSystem fileSystem, InMemoryDirectory parent, Uri path, string name)
        {
            _parent = parent;
            Name = name;
            RootFileSystem = FileSystem = fileSystem;
            Path = path;
            CreationTimeUtc = LastWriteTimeUtc = DateTime.UtcNow;
        }

        public string Name { get; }
        public IFileSystem RootFileSystem { get; }
        public IFileSystem FileSystem { get; }
        public ICollection Parent => _parent;
        public Uri Path { get; }
        public DateTime LastWriteTimeUtc { get; protected set; }
        protected InMemoryDirectory InMemoryParent => _parent;
        protected DateTime CreationTimeUtc { get; set; }

        public IAsyncEnumerable<IUntypedReadableProperty> GetProperties()
        {
            return new EntryProperties(this, GetLiveProperties(), GetPredefinedDeadProperties(), FileSystem.PropertyStore);
        }

        public abstract Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken);

        protected virtual IEnumerable<ILiveProperty> GetLiveProperties()
        {
            var properties = new List<ILiveProperty>()
            {
                this.GetResourceTypeProperty(),
                new LastModifiedProperty(ct => Task.FromResult(LastWriteTimeUtc), (v, ct) => Task.FromResult(LastWriteTimeUtc = v)),
                new CreationDateProperty(ct => Task.FromResult(CreationTimeUtc), (v, ct) => Task.FromResult(CreationTimeUtc = v)),
            };
            return properties;
        }

        protected virtual IEnumerable<IDeadProperty> GetPredefinedDeadProperties()
        {
            var displayProperty = FileSystem.PropertyStore?.Create(this, DisplayNameProperty.PropertyName);
            if (displayProperty != null)
                yield return displayProperty;
        }
    }
}
