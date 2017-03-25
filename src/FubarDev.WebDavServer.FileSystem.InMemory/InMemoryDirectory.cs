// <copyright file="InMemoryDirectory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;

namespace FubarDev.WebDavServer.FileSystem.InMemory
{
    /// <summary>
    /// An in-memory implementation of a WebDAV collection
    /// </summary>
    public class InMemoryDirectory : InMemoryEntry, ICollection, IRecusiveChildrenCollector
    {
        private readonly Dictionary<string, InMemoryEntry> _children = new Dictionary<string, InMemoryEntry>(StringComparer.OrdinalIgnoreCase);

        private readonly bool _isRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryDirectory"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system this collection belongs to</param>
        /// <param name="parent">The parent collection</param>
        /// <param name="path">The root-relative path of this collection</param>
        /// <param name="name">The name of the collection</param>
        /// <param name="isRoot">Is this the file systems root directory?</param>
        public InMemoryDirectory(InMemoryFileSystem fileSystem, ICollection parent, Uri path, string name, bool isRoot = false)
            : base(fileSystem, parent, path, name)
        {
            _isRoot = isRoot;
        }

        /// <inheritdoc />
        public override async Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken)
        {
            if (InMemoryFileSystem.IsReadOnly)
                throw new UnauthorizedAccessException("Failed to modify a read-only file system");

            if (_isRoot)
                throw new UnauthorizedAccessException("Cannot remove the file systems root collection");

            if (InMemoryParent == null)
                throw new InvalidOperationException("The collection must belong to a collection");

            if (InMemoryParent.Remove(Name))
            {
                var propStore = FileSystem.PropertyStore;
                if (propStore != null)
                {
                    await propStore.RemoveAsync(this, cancellationToken).ConfigureAwait(false);
                }

                return new DeleteResult(WebDavStatusCode.OK, null);
            }

            return new DeleteResult(WebDavStatusCode.NotFound, this);
        }

        /// <inheritdoc />
        public Task<IEntry> GetChildAsync(string name, CancellationToken ct)
        {
            InMemoryEntry entry;
            _children.TryGetValue(name, out entry);

            var coll = entry as ICollection;
            if (coll != null)
                return coll.GetMountTargetEntryAsync(InMemoryFileSystem);

            return Task.FromResult<IEntry>(entry);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IEntry>> GetChildrenAsync(CancellationToken ct)
        {
            var result = new List<IEntry>();
            foreach (var child in _children.Values)
            {
                var coll = child as ICollection;
                if (coll != null)
                {
                    result.Add(await coll.GetMountTargetAsync(InMemoryFileSystem).ConfigureAwait(false));
                }
                else
                {
                    result.Add(child);
                }
            }

            return result;
        }

        /// <inheritdoc />
        public Task<IDocument> CreateDocumentAsync(string name, CancellationToken ct)
        {
            return Task.FromResult<IDocument>(CreateDocument(name));
        }

        /// <inheritdoc />
        public Task<ICollection> CreateCollectionAsync(string name, CancellationToken ct)
        {
            if (InMemoryFileSystem.IsReadOnly)
                throw new UnauthorizedAccessException("Failed to modify a read-only file system");
            if (_children.ContainsKey(name))
                throw new IOException("Document or collection with the same name already exists");
            var newItem = new InMemoryDirectory(InMemoryFileSystem, this, Path.AppendDirectory(name), name);
            _children.Add(newItem.Name, newItem);
            ETag = new EntityTag(false);
            return Task.FromResult<ICollection>(newItem);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<IEntry> GetEntries(int maxDepth)
        {
            return this.EnumerateEntries(maxDepth);
        }

        /// <summary>
        /// Creates a document
        /// </summary>
        /// <param name="name">The name of the document to create</param>
        /// <returns>The created document</returns>
        /// <exception cref="UnauthorizedAccessException">The file system is read-only</exception>
        /// <exception cref="IOException">Document or collection with the same name already exists</exception>
        public InMemoryFile CreateDocument(string name)
        {
            if (InMemoryFileSystem.IsReadOnly)
                throw new UnauthorizedAccessException("Failed to modify a read-only file system");
            if (_children.ContainsKey(name))
                throw new IOException("Document or collection with the same name already exists");
            var newItem = new InMemoryFile(InMemoryFileSystem, this, Path.Append(name, false), name);
            _children.Add(newItem.Name, newItem);
            ETag = new EntityTag(false);
            return newItem;
        }

        /// <summary>
        /// Creates a new collection
        /// </summary>
        /// <param name="name">The name of the collection to create</param>
        /// <returns>The created collection</returns>
        /// <exception cref="UnauthorizedAccessException">The file system is read-only</exception>
        /// <exception cref="IOException">Document or collection with the same name already exists</exception>
        public InMemoryDirectory CreateCollection(string name)
        {
            if (InMemoryFileSystem.IsReadOnly)
                throw new UnauthorizedAccessException("Failed to modify a read-only file system");
            if (_children.ContainsKey(name))
                throw new IOException("Document or collection with the same name already exists");
            var newItem = new InMemoryDirectory(InMemoryFileSystem, this, Path.AppendDirectory(name), name);
            _children.Add(newItem.Name, newItem);
            ETag = new EntityTag(false);
            return newItem;
        }

        internal bool Remove(string name)
        {
            if (InMemoryFileSystem.IsReadOnly)
                throw new UnauthorizedAccessException("Failed to modify a read-only file system");
            return _children.Remove(name);
        }
    }
}
