// <copyright file="SQLiteCollection.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.FileSystem.SQLite
{
    /// <summary>
    /// A <see cref="SQLitePCL"/> based implementation of a WebDAV collection.
    /// </summary>
    internal class SQLiteCollection : SQLiteEntry, ICollection, IRecursiveChildrenCollector
    {
        private readonly bool _isRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteCollection"/> class.
        /// </summary>
        /// <param name="dbFileSystem">The file system this collection belongs to.</param>
        /// <param name="parent">The parent collection.</param>
        /// <param name="info">The directory information.</param>
        /// <param name="path">The root-relative path of this collection.</param>
        /// <param name="name">The entry name (<see langword="null"/> when <see cref="FileEntry.Name"/> of <see cref="SQLiteEntry.Info"/> should be used).</param>
        /// <param name="isRoot">Indicates whether this is the file systems root directory.</param>
        public SQLiteCollection(
            SQLiteFileSystem dbFileSystem,
            ICollection? parent,
            FileEntry info,
            Uri path,
            string? name,
            bool isRoot = false)
            : base(dbFileSystem, parent, info, path, name)
        {
            _isRoot = isRoot;
        }

        /// <inheritdoc />
        public async Task<IEntry?> GetChildAsync(string name, CancellationToken ct)
        {
            var childId = Path.Append(name, false).OriginalString.ToLowerInvariant();
            var childEntry = Connection.Table<FileEntry>().FirstOrDefault(x => x.Id == childId);
            if (childEntry == null)
            {
                return null;
            }

            var entry = CreateEntry(childEntry);

            if (entry is ICollection coll)
            {
                return await coll.GetMountTargetEntryAsync(DbFileSystem);
            }

            return entry;
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
                if (entry is ICollection coll)
                {
                    entry = await coll.GetMountTargetEntryAsync(DbFileSystem);
                }

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
                Path = Path.OriginalString.ToLowerInvariant(),
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
                Path = Path.OriginalString.ToLowerInvariant(),
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
            {
                throw new UnauthorizedAccessException("Cannot remove the file systems root collection");
            }

            var propStore = FileSystem.PropertyStore;
            if (propStore != null)
            {
                await propStore.RemoveAsync(this, cancellationToken).ConfigureAwait(false);
            }

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
            {
                return new SQLiteDocument(DbFileSystem, this, entry, Path.Append(entry.Name, false));
            }

            return new SQLiteCollection(DbFileSystem, this, entry, Path.AppendDirectory(entry.Name), null);
        }
    }
}
