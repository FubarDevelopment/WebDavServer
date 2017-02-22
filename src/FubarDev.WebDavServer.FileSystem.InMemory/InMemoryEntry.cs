// <copyright file="InMemoryEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.Props;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Live;

namespace FubarDev.WebDavServer.FileSystem.InMemory
{
    public abstract class InMemoryEntry : IEntry, IEntityTagEntry
    {
        private readonly InMemoryDirectory _parent;

        protected InMemoryEntry(InMemoryFileSystem fileSystem, InMemoryDirectory parent, Uri path, string name)
        {
            _parent = parent;
            Name = name;
            RootFileSystem = FileSystem = InMemoryFileSystem = fileSystem;
            Path = path;
            CreationTimeUtc = LastWriteTimeUtc = DateTime.UtcNow;
        }

        public string Name { get; }

        public IFileSystem RootFileSystem { get; }

        public IFileSystem FileSystem { get; }

        public ICollection Parent => _parent;

        public Uri Path { get; }

        public DateTime LastWriteTimeUtc { get; protected set; }

        public DateTime CreationTimeUtc { get; protected set; }

        /// <inheritdoc />
        public EntityTag ETag { get; protected set; } = new EntityTag(false);

        protected InMemoryFileSystem InMemoryFileSystem { get; }

        protected InMemoryDirectory InMemoryParent => _parent;

        public IAsyncEnumerable<IUntypedReadableProperty> GetProperties(int? maxCost)
        {
            return new EntryProperties(this, GetLiveProperties(), GetPredefinedDeadProperties(), FileSystem.PropertyStore, maxCost);
        }

        /// <inheritdoc />
        public Task<EntityTag> UpdateETagAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(ETag = new EntityTag(false));
        }

        public abstract Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken);

        protected virtual IEnumerable<ILiveProperty> GetLiveProperties()
        {
            var properties = new List<ILiveProperty>()
            {
                this.GetResourceTypeProperty(),
                new LastModifiedProperty(ct => Task.FromResult(LastWriteTimeUtc), (v, ct) => Task.FromResult(LastWriteTimeUtc = v)),
                new CreationDateProperty(ct => Task.FromResult(CreationTimeUtc), (v, ct) => Task.FromResult(CreationTimeUtc = v)),
                new LockDiscoveryProperty(FileSystem.LockManager, this),
            };
            return properties;
        }

        protected virtual IEnumerable<IDeadProperty> GetPredefinedDeadProperties()
        {
            yield return InMemoryFileSystem
                .DeadPropertyFactory.Create(FileSystem.PropertyStore, this, DisplayNameProperty.PropertyName);
            yield return new GetETagProperty(FileSystem.PropertyStore, this, 0);
        }
    }
}
