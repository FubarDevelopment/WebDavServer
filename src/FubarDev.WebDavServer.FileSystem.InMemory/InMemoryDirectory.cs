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
    public class InMemoryDirectory : InMemoryEntry, ICollection
    {
        private readonly Dictionary<string, InMemoryEntry> _children = new Dictionary<string, InMemoryEntry>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryDirectory"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system this collection belongs to</param>
        /// <param name="parent">The parent collection</param>
        /// <param name="path">The root-relative path of this collection</param>
        /// <param name="name">The name of the collection</param>
        public InMemoryDirectory(InMemoryFileSystem fileSystem, InMemoryDirectory parent, Uri path, string name)
            : base(fileSystem, parent, path, name)
        {
        }

        /// <inheritdoc />
        public override async Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken)
        {
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
            return Task.FromResult<IEntry>(entry);
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<IEntry>> GetChildrenAsync(CancellationToken ct)
        {
            return Task.FromResult<IReadOnlyCollection<IEntry>>(_children.Values.ToList());
        }

        /// <inheritdoc />
        public Task<IDocument> CreateDocumentAsync(string name, CancellationToken ct)
        {
            if (_children.ContainsKey(name))
                throw new IOException("Document or collection with the same name already exists");
            var newItem = new InMemoryFile(InMemoryFileSystem, this, Path.Append(name, false), name);
            _children.Add(newItem.Name, newItem);
            ETag = new EntityTag(false);
            return Task.FromResult<IDocument>(newItem);
        }

        /// <inheritdoc />
        public Task<ICollection> CreateCollectionAsync(string name, CancellationToken ct)
        {
            if (_children.ContainsKey(name))
                throw new IOException("Document or collection with the same name already exists");
            var newItem = new InMemoryDirectory(InMemoryFileSystem, this, Path.AppendDirectory(name), name);
            _children.Add(newItem.Name, newItem);
            ETag = new EntityTag(false);
            return Task.FromResult<ICollection>(newItem);
        }

        internal bool Remove(string name)
        {
            return _children.Remove(name);
        }
    }
}
