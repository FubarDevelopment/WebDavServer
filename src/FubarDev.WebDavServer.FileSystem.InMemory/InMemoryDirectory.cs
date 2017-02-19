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
using FubarDev.WebDavServer.Props.Converters;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Live;

namespace FubarDev.WebDavServer.FileSystem.InMemory
{
    public class InMemoryDirectory : InMemoryEntry, ICollection
    {
        private readonly Dictionary<string, InMemoryEntry> _children = new Dictionary<string, InMemoryEntry>(StringComparer.OrdinalIgnoreCase);

        public InMemoryDirectory(InMemoryFileSystem fileSystem, InMemoryDirectory parent, Uri path, string name)
            : base(fileSystem, parent, path, name)
        {
        }

        public override async Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken)
        {
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

        public Task<IEntry> GetChildAsync(string name, CancellationToken ct)
        {
            InMemoryEntry entry;
            _children.TryGetValue(name, out entry);
            return Task.FromResult<IEntry>(entry);
        }

        public Task<IReadOnlyCollection<IEntry>> GetChildrenAsync(CancellationToken ct)
        {
            return Task.FromResult<IReadOnlyCollection<IEntry>>(_children.Values.ToList());
        }

        public Task<IDocument> CreateDocumentAsync(string name, CancellationToken ct)
        {
            if (_children.ContainsKey(name))
                throw new IOException("Document or collection with the same name already exists");
            var newItem = new InMemoryFile(InMemoryFileSystem, this, Path.Append(name, false), name);
            _children.Add(newItem.Name, newItem);
            ETag = new EntityTag(false);
            return Task.FromResult<IDocument>(newItem);
        }

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

        protected override IEnumerable<ILiveProperty> GetLiveProperties()
        {
            return base.GetLiveProperties()
                .Concat(new ILiveProperty[]
                {
                    new ContentLengthProperty(ct => Task.FromResult(0L)),
                });
        }

        protected override IEnumerable<IDeadProperty> GetPredefinedDeadProperties()
        {
            var contentType = InMemoryFileSystem.DeadPropertyFactory
                .Create(FileSystem.PropertyStore, this, GetContentTypeProperty.PropertyName);
            contentType.Init(new StringConverter().ToElement(GetContentTypeProperty.PropertyName, Utils.MimeTypesMap.FolderContentType));
            return base.GetPredefinedDeadProperties()
                .Concat(new[]
                {
                    contentType,
                });
        }
    }
}
