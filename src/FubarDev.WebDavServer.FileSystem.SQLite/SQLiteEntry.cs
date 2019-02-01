﻿// <copyright file="SQLiteEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.Props;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Live;

using JetBrains.Annotations;

using SQLite;

namespace FubarDev.WebDavServer.FileSystem.SQLite
{
    /// <summary>
    /// A <see cref="SQLitePCL"/> based implementation of a WebDAV entry (collection or document)
    /// </summary>
    internal abstract class SQLiteEntry : IEntry, IEntityTagEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteEntry"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system this entry belongs to</param>
        /// <param name="parent">The parent collection</param>
        /// <param name="info">The file system information</param>
        /// <param name="path">The root-relative path of this entry</param>
        /// <param name="name">The entry name (<see langword="null"/> when <see cref="FileEntry.Name"/> of <see cref="SQLiteEntry.Info"/> should be used)</param>
        protected SQLiteEntry(SQLiteFileSystem fileSystem, ICollection parent, FileEntry info, Uri path, [CanBeNull] string name)
        {
            Parent = parent;
            Info = info;
            SQLiteFileSystem = fileSystem;
            Path = path;
            ETag = EntityTag.Parse(Info.ETag).Single();
            Name = name ?? info.Name;
        }

        /// <summary>
        /// Gets the file system information of this entry
        /// </summary>
        public FileEntry Info { get; }

        /// <summary>
        /// Gets the file system this entry belongs to
        /// </summary>
        public SQLiteFileSystem SQLiteFileSystem { get; }

        /// <summary>
        /// Gets the SQLite connection
        /// </summary>
        public SQLiteConnection Connection => SQLiteFileSystem.Connection;

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public IFileSystem FileSystem => SQLiteFileSystem;

        /// <inheritdoc />
        public ICollection Parent { get; }

        /// <inheritdoc />
        public Uri Path { get; }

        /// <summary>
        /// Gets the last time this entry was modified.
        /// </summary>
        public DateTime LastWriteTimeUtc => Info.LastWriteTimeUtc;

        /// <summary>
        /// Gets the time this entry was created.
        /// </summary>
        public DateTime CreationTimeUtc => Info.CreationTimeUtc;

        /// <inheritdoc />
        public EntityTag ETag { get; private set; }

        /// <inheritdoc />
        public Task<EntityTag> UpdateETagAsync(CancellationToken cancellationToken)
        {
            var newETag = ETag.Update();
            Info.ETag = newETag.ToString();
            try
            {
                Connection.Update(Info);
            }
            catch
            {
                Info.ETag = ETag.ToString();
                throw;
            }

            ETag = newETag;
            return Task.FromResult(newETag);
        }

        /// <inheritdoc />
        public abstract Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Sets the last write time.
        /// </summary>
        /// <param name="lastWriteTime">The new last write time.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The async task.</returns>
        [JetBrains.Annotations.NotNull]
        public Task SetLastWriteTimeUtcAsync(DateTime lastWriteTime, CancellationToken cancellationToken)
        {
            if (Info.LastWriteTimeUtc != lastWriteTime)
            {
                Info.LastWriteTimeUtc = lastWriteTime;
                Info.ETag = EntityTag.Parse(Info.ETag).Single().Update().ToString();
                Connection.Update(Info);
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Sets the creation time.
        /// </summary>
        /// <param name="creationTime">The new creation time.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The async task.</returns>
        [JetBrains.Annotations.NotNull]
        public Task SetCreationTimeUtcAsync(DateTime creationTime, CancellationToken cancellationToken)
        {
            if (Info.CreationTimeUtc != creationTime)
            {
                Info.CreationTimeUtc = creationTime;
                Info.ETag = EntityTag.Parse(Info.ETag).Single().Update().ToString();
                Connection.Update(Info);
            }

            return Task.FromResult(0);
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
