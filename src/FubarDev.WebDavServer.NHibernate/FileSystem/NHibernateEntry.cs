// <copyright file="NHibernateEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.NHibernate.Models;

using JetBrains.Annotations;

using NHibernate;

namespace FubarDev.WebDavServer.NHibernate.FileSystem
{
    /// <summary>
    /// A <see cref="NHibernate"/> based implementation of a WebDAV entry (collection or document)
    /// </summary>
    internal abstract class NHibernateEntry : IEntry, IEntityTagEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateEntry"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system this entry belongs to</param>
        /// <param name="parent">The parent collection</param>
        /// <param name="info">The file system information</param>
        /// <param name="path">The root-relative path of this entry</param>
        /// <param name="name">The entry name (<see langword="null"/> when <see cref="FileEntry.Name"/> should be used)</param>
        protected NHibernateEntry(NHibernateFileSystem fileSystem, ICollection parent, FileEntry info, Uri path, [CanBeNull] string name)
        {
            Parent = parent;
            Info = info;
            NHibernateFileSystem = fileSystem;
            Path = path;
            Name = name ?? info.Name;
        }

        /// <summary>
        /// Gets the file system information of this entry
        /// </summary>
        public FileEntry Info { get; }

        /// <summary>
        /// Gets the file system this entry belongs to
        /// </summary>
        public NHibernateFileSystem NHibernateFileSystem { get; }

        /// <summary>
        /// Gets the SQLite connection
        /// </summary>
        protected ISession Connection => NHibernateFileSystem.Connection;

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public IFileSystem FileSystem => NHibernateFileSystem;

        /// <inheritdoc />
        public ICollection Parent { get; }

        /// <inheritdoc />
        public Uri Path { get; }

        /// <inheritdoc />
        public DateTime LastWriteTimeUtc => Info.LastWriteTimeUtc;

        /// <inheritdoc />
        public DateTime CreationTimeUtc => Info.CreationTimeUtc;

        /// <inheritdoc />
        public EntityTag ETag => Info.ETag;

        /// <inheritdoc />
        public async Task<EntityTag> UpdateETagAsync(CancellationToken cancellationToken)
        {
            var oldETag = Info.ETag;
            Info.ETag = oldETag.Update();
            try
            {
                await Connection.UpdateAsync(Info, cancellationToken).ConfigureAwait(false);
                await Connection.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                Info.ETag = oldETag;
                throw;
            }

            return Info.ETag;
        }

        /// <inheritdoc />
        public abstract Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken);

        /// <inheritdoc />
        public async Task SetLastWriteTimeUtcAsync(DateTime lastWriteTime, CancellationToken cancellationToken)
        {
            if (Info.LastWriteTimeUtc == lastWriteTime)
                return;

            Info.LastWriteTimeUtc = lastWriteTime;
            await Connection.UpdateAsync(Info, cancellationToken).ConfigureAwait(false);
            await Connection.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task SetCreationTimeUtcAsync(DateTime creationTime, CancellationToken cancellationToken)
        {
            if (Info.CreationTimeUtc == creationTime)
                return;

            Info.CreationTimeUtc = creationTime;
            await Connection.UpdateAsync(Info, cancellationToken).ConfigureAwait(false);
            await Connection.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
