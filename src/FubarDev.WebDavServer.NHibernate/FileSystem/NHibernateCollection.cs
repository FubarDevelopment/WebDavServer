// <copyright file="NHibernateCollection.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.NHibernate.Models;

using JetBrains.Annotations;

using NHibernate.Linq;

namespace FubarDev.WebDavServer.NHibernate.FileSystem
{
    /// <summary>
    /// A <see cref="NHibernate"/> based implementation of a WebDAV collection
    /// </summary>
    internal class NHibernateCollection : NHibernateEntry, ICollection, IRecusiveChildrenCollector
    {
        private readonly bool _isRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateCollection"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system this collection belongs to</param>
        /// <param name="parent">The parent collection</param>
        /// <param name="info">The directory information</param>
        /// <param name="path">The root-relative path of this collection</param>
        /// <param name="name">The entry name (<see langword="null"/> when <see cref="FileEntry.Name"/> should be used)</param>
        /// <param name="isRoot">Is this the file systems root directory?</param>
        public NHibernateCollection(
            [NotNull] NHibernateFileSystem fileSystem,
            [CanBeNull] ICollection parent,
            [NotNull] FileEntry info,
            [NotNull] Uri path,
            [CanBeNull] string name,
            bool isRoot = false)
            : base(fileSystem, parent, info, path, name)
        {
            _isRoot = isRoot;
        }

        /// <inheritdoc />
        public async Task<IEntry> GetChildAsync(string name, CancellationToken ct)
        {
            var childEntry = await Connection.Query<FileEntry>()
                .Where(x => x.ParentId == Info.Id && x.InvariantName == name.ToLowerInvariant())
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
            if (childEntry == null)
                return null;

            var entry = CreateEntry(childEntry);

            if (entry is ICollection coll)
                return await coll.GetMountTargetEntryAsync(NHibernateFileSystem);

            return entry;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IEntry>> GetChildrenAsync(CancellationToken ct)
        {
            var result = new List<IEntry>();
            var entries = await Connection.Query<FileEntry>()
                .Where(x => x.ParentId == Info.Id)
                .ToListAsync(ct)
                .ConfigureAwait(false);
            foreach (var info in entries)
            {
                ct.ThrowIfCancellationRequested();
                var entry = CreateEntry(info);
                if (entry is ICollection coll)
                    entry = await coll.GetMountTargetEntryAsync(NHibernateFileSystem);
                result.Add(entry);
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<IDocument> CreateDocumentAsync(string name, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var invariantName = name.ToLowerInvariant();
            var newEntry = new FileEntry()
            {
                Id = Guid.NewGuid(),
                ParentId = Info.Id,
                IsCollection = false,
                Name = name,
                InvariantName = invariantName,
                LastWriteTimeUtc = now,
                CreationTimeUtc = now,
                Properties = new Dictionary<string, PropertyEntry>(),
            };

            await Connection.SaveAsync(newEntry, cancellationToken).ConfigureAwait(false);
            await Connection.FlushAsync(cancellationToken).ConfigureAwait(false);

            return (IDocument)CreateEntry(newEntry);
        }

        /// <inheritdoc />
        public async Task<ICollection> CreateCollectionAsync(string name, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var invariantName = name.ToLowerInvariant();
            var newEntry = new FileEntry()
            {
                Id = Guid.NewGuid(),
                ParentId = Info.Id,
                IsCollection = true,
                Name = name,
                InvariantName = invariantName,
                LastWriteTimeUtc = now,
                CreationTimeUtc = now,
                Properties = new Dictionary<string, PropertyEntry>(),
            };

            await Connection.SaveAsync(newEntry, cancellationToken).ConfigureAwait(false);
            await Connection.FlushAsync(cancellationToken).ConfigureAwait(false);

            return (ICollection)CreateEntry(newEntry);
        }

        /// <inheritdoc />
        public override async Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken)
        {
            if (_isRoot)
                throw new UnauthorizedAccessException("Cannot remove the file systems root collection");

            var propStore = FileSystem.PropertyStore;
            if (propStore != null)
                await propStore.RemoveAsync(this, cancellationToken).ConfigureAwait(false);

            using (var trans = Connection.BeginTransaction())
            {
                await Connection.DeleteAsync(Info, cancellationToken).ConfigureAwait(false);
                await trans.CommitAsync(cancellationToken).ConfigureAwait(false);
            }

            return new DeleteResult(WebDavStatusCode.OK, null);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<IEntry> GetEntries(int maxDepth)
        {
            return this.EnumerateEntries(maxDepth);
        }

        private IEntry CreateEntry(FileEntry entry)
        {
            if (!entry.IsCollection)
                return new NHibernateDocument(NHibernateFileSystem, this, entry, Path.Append(entry.Name, false));

            return new NHibernateCollection(NHibernateFileSystem, this, entry, Path.AppendDirectory(entry.Name), null);
        }
    }
}
