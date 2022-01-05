// <copyright file="InMemoryEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Models;
using FubarDev.WebDavServer.Props;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Live;

namespace FubarDev.WebDavServer.FileSystem.InMemory
{
    /// <summary>
    /// Am in-memory implementation of a WebDAV entry (collection or document).
    /// </summary>
    public abstract class InMemoryEntry : IEntityTagEntry
    {
        private readonly ICollection? _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEntry"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system this entry belongs to.</param>
        /// <param name="parent">The parent collection.</param>
        /// <param name="path">The root-relative path of this entry.</param>
        /// <param name="name">The name of the entry.</param>
        protected InMemoryEntry(InMemoryFileSystem fileSystem, ICollection? parent, Uri path, string name)
        {
            _parent = parent;
            Name = name;
            FileSystem = InMemoryFileSystem = fileSystem;
            Path = path;
            CreationTimeUtc = LastWriteTimeUtc = DateTime.UtcNow;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public IFileSystem FileSystem { get; }

        /// <inheritdoc />
        public ICollection? Parent => _parent;

        /// <inheritdoc />
        public Uri Path { get; }

        /// <summary>
        /// Gets or sets the last time this entry was modified.
        /// </summary>
        public DateTime LastWriteTimeUtc { get; protected set; }

        /// <summary>
        /// Gets or sets the time this entry was created.
        /// </summary>
        public DateTime CreationTimeUtc { get; protected set; }

        /// <inheritdoc />
        public EntityTag ETag { get; protected set; } = new EntityTag(false);

        /// <summary>
        /// Gets the file system.
        /// </summary>
        protected InMemoryFileSystem InMemoryFileSystem { get; }

        /// <summary>
        /// Gets the parent collection.
        /// </summary>
        protected InMemoryDirectory? InMemoryParent => _parent as InMemoryDirectory;

        /// <inheritdoc />
        public Task<EntityTag> UpdateETagAsync(CancellationToken cancellationToken)
        {
            if (InMemoryFileSystem.IsReadOnly)
            {
                throw new UnauthorizedAccessException("Failed to modify a read-only file system");
            }

            return Task.FromResult(ETag = new EntityTag(false));
        }

        /// <inheritdoc />
        public abstract Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Sets the last write time.
        /// </summary>
        /// <param name="lastWriteTime">The new last write time.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The async task.</returns>
        public Task SetLastWriteTimeUtcAsync(DateTime lastWriteTime, CancellationToken cancellationToken)
        {
            if (InMemoryFileSystem.IsReadOnly)
            {
                throw new UnauthorizedAccessException("Failed to modify a read-only file system");
            }

            LastWriteTimeUtc = lastWriteTime;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the creation time.
        /// </summary>
        /// <param name="creationTime">The new creation time.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The async task.</returns>
        public Task SetCreationTimeUtcAsync(DateTime creationTime, CancellationToken cancellationToken)
        {
            if (InMemoryFileSystem.IsReadOnly)
            {
                throw new UnauthorizedAccessException("Failed to modify a read-only file system");
            }

            CreationTimeUtc = creationTime;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public IEnumerable<IUntypedReadableProperty> GetLiveProperties()
        {
            yield return new LastModifiedProperty(LastWriteTimeUtc, SetLastWriteTimeUtcAsync);
            yield return new CreationDateProperty(CreationTimeUtc, (value, ct) => SetCreationTimeUtcAsync(value.UtcDateTime, ct));
            yield return new GetETagProperty(FileSystem.PropertyStore, this);
        }
    }
}
