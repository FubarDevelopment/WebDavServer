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

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem.InMemory
{
    /// <summary>
    /// Am in-memory implementation of a WebDAV entry (collection or document)
    /// </summary>
    public abstract class InMemoryEntry : IEntry, IEntityTagEntry
    {
        private readonly InMemoryDirectory _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEntry"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system this entry belongs to</param>
        /// <param name="parent">The parent collection</param>
        /// <param name="path">The root-relative path of this entry</param>
        /// <param name="name">The name of the entry</param>
        protected InMemoryEntry(InMemoryFileSystem fileSystem, InMemoryDirectory parent, Uri path, string name)
        {
            _parent = parent;
            Name = name;
            RootFileSystem = FileSystem = InMemoryFileSystem = fileSystem;
            Path = path;
            CreationTimeUtc = LastWriteTimeUtc = DateTime.UtcNow;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public IFileSystem RootFileSystem { get; }

        /// <inheritdoc />
        public IFileSystem FileSystem { get; }

        /// <inheritdoc />
        public ICollection Parent => _parent;

        /// <inheritdoc />
        public Uri Path { get; }

        /// <inheritdoc />
        public DateTime LastWriteTimeUtc { get; protected set; }

        /// <inheritdoc />
        public DateTime CreationTimeUtc { get; protected set; }

        /// <inheritdoc />
        public EntityTag ETag { get; protected set; } = new EntityTag(false);

        /// <summary>
        /// Gets the file system
        /// </summary>
        [NotNull]
        protected InMemoryFileSystem InMemoryFileSystem { get; }

        /// <summary>
        /// Gets the parent collection
        /// </summary>
        [CanBeNull]
        protected InMemoryDirectory InMemoryParent => _parent;

        /// <inheritdoc />
        public IAsyncEnumerable<IUntypedReadableProperty> GetProperties(int? maxCost)
        {
            return new EntryProperties(this, GetLiveProperties(), GetPredefinedDeadProperties(), FileSystem.PropertyStore, maxCost);
        }

        /// <inheritdoc />
        public Task<EntityTag> UpdateETagAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(ETag = new EntityTag(false));
        }

        /// <inheritdoc />
        public abstract Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the default live properties of this entry
        /// </summary>
        /// <returns>The enumeration of live properties belonging to this entry</returns>
        protected virtual IEnumerable<ILiveProperty> GetLiveProperties()
        {
            var properties = new List<ILiveProperty>()
            {
                this.GetResourceTypeProperty(),
                new LockDiscoveryProperty(this),
                new SupportedLockProperty(this),
                new LastModifiedProperty(LastWriteTimeUtc, (v, ct) => Task.FromResult(LastWriteTimeUtc = v)),
                new CreationDateProperty(CreationTimeUtc, (v, ct) => Task.FromResult(CreationTimeUtc = v)),
            };
            return properties;
        }

        /// <summary>
        /// Gets the default dead properties of this entry
        /// </summary>
        /// <returns>The enumeration of dead properties belonging to this entry</returns>
        protected virtual IEnumerable<IDeadProperty> GetPredefinedDeadProperties()
        {
            yield return InMemoryFileSystem
                .DeadPropertyFactory.Create(FileSystem.PropertyStore, this, DisplayNameProperty.PropertyName);
            yield return new GetETagProperty(FileSystem.PropertyStore, this, 0);
        }
    }
}
