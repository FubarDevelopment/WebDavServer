﻿// <copyright file="InMemoryEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model.Headers;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem.InMemory
{
    /// <summary>
    /// Am in-memory implementation of a WebDAV entry (collection or document)
    /// </summary>
    public abstract class InMemoryEntry : IEntry, IEntityTagEntry
    {
        private readonly ICollection _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEntry"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system this entry belongs to</param>
        /// <param name="parent">The parent collection</param>
        /// <param name="path">The root-relative path of this entry</param>
        /// <param name="name">The name of the entry</param>
        protected InMemoryEntry(InMemoryFileSystem fileSystem, ICollection parent, Uri path, string name)
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
        protected InMemoryDirectory InMemoryParent => _parent as InMemoryDirectory;

        /// <inheritdoc />
        public Task<EntityTag> UpdateETagAsync(CancellationToken cancellationToken)
        {
            if (InMemoryFileSystem.IsReadOnly)
                throw new UnauthorizedAccessException("Failed to modify a read-only file system");

            return Task.FromResult(ETag = new EntityTag(false));
        }

        /// <inheritdoc />
        public abstract Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken);

        /// <inheritdoc />
        public Task SetLastWriteTimeUtcAsync(DateTime lastWriteTime, CancellationToken cancellationToken)
        {
            if (InMemoryFileSystem.IsReadOnly)
                throw new UnauthorizedAccessException("Failed to modify a read-only file system");

            LastWriteTimeUtc = lastWriteTime;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SetCreationTimeUtcAsync(DateTime creationTime, CancellationToken cancellationToken)
        {
            if (InMemoryFileSystem.IsReadOnly)
                throw new UnauthorizedAccessException("Failed to modify a read-only file system");

            CreationTimeUtc = creationTime;
            return Task.CompletedTask;
        }
    }
}
