// <copyright file="SQLiteCollection.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem.SQLite
{
    /// <summary>
    /// A <see cref="SQLitePCL"/> based implementation of a WebDAV collection
    /// </summary>
    internal class SQLiteCollection : SQLiteEntry, ICollection, IRecusiveChildrenCollector
    {
        private readonly bool _isRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteCollection"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system this collection belongs to</param>
        /// <param name="parent">The parent collection</param>
        /// <param name="info">The directory information</param>
        /// <param name="path">The root-relative path of this collection</param>
        /// <param name="name">The entry name (<see langword="null"/> when <see cref="FileEntry.Name"/> of <see cref="SQLiteEntry.Info"/> should be used)</param>
        /// <param name="isRoot">Is this the file systems root directory?</param>
        public SQLiteCollection(
            [NotNull] SQLiteFileSystem fileSystem,
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
        public Task<IEntry> GetChildAsync(string name, CancellationToken ct)
        {
            var childId = Path.Append(name, false).OriginalString.ToLowerInvariant();
            var childEntry = Connection.Table<FileEntry>().FirstOrDefault(x => x.Id == childId);
            if (childEntry == null)
                return Task.FromResult<IEntry>(null);

            var entry = CreateEntry(childEntry);

            var coll = entry as ICollection;
            if (coll != null)
                return coll.GetMountTargetEntryAsync(SQLiteFileSystem);

            return Task.FromResult(entry);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IEntry>> GetChildrenAsync(CancellationToken ct)
        {
            var result = new List<IEntry>();
            var path = Path.OriginalString.ToLowerInvariant();
            var entries = Connection.Table<FileEntry>().Where(x => x.Path == path && x.Id != Info.Id).ToList();
            foreach (var info in entries)
            {
                ct.ThrowIfCancellationRequested();
                var entry = CreateEntry(info);
                var coll = entry as ICollection;
                if (coll != null)
                    entry = await coll.GetMountTargetEntryAsync(SQLiteFileSystem);
                result.Add(entry);
            }

            return result;
        }

        /// <inheritdoc />
        public Task<IDocument> CreateDocumentAsync(string name, CancellationToken cancellationToken)
        {
            var childId = Path.Append(name, false).OriginalString.ToLowerInvariant();
            var newEntry = new FileEntry()
            {
                Id = childId,
                IsCollection = false,
                Name = name,
                Path = Path.OriginalString,
            };
            Connection.Insert(newEntry);
            return Task.FromResult((IDocument)CreateEntry(newEntry));
        }

        /// <inheritdoc />
        public Task<ICollection> CreateCollectionAsync(string name, CancellationToken cancellationToken)
        {
            var childId = Path.Append(name, false).OriginalString.ToLowerInvariant();
            var newEntry = new FileEntry()
            {
                Id = childId,
                IsCollection = true,
                Name = name,
                Path = Path.OriginalString,
            };
            try
            {
                Connection.Insert(newEntry);
            }
            catch (global::SQLite.SQLiteException ex)
            {
                throw new IOException(ex.Message, ex);
            }

            return Task.FromResult((ICollection)CreateEntry(newEntry));
        }

        /// <inheritdoc />
        public override async Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken)
        {
            if (_isRoot)
                throw new UnauthorizedAccessException("Cannot remove the file systems root collection");

            var propStore = FileSystem.PropertyStore;
            if (propStore != null)
                await propStore.RemoveAsync(this, cancellationToken).ConfigureAwait(false);

            Connection.RunInTransaction(() =>
            {
                // Delete all data
                Connection
                    .CreateCommand(
                        "delete from filesystementrydata where id in (select e.id from filesystementries e where e.id=? or e.path=?)",
                        Info.Id,
                        Path.OriginalString)
                    .ExecuteNonQuery();

                // Delete the entries
                Connection
                    .CreateCommand(
                        "delete from filesystementries where id=? or path=?",
                        Info.Id,
                        Path.OriginalString)
                    .ExecuteNonQuery();
            });

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
                return new SQLiteDocument(SQLiteFileSystem, this, entry, Path.Append(entry.Name, false));

            return new SQLiteCollection(SQLiteFileSystem, this, entry, Path.AppendDirectory(entry.Name), null);
        }
    }
}
