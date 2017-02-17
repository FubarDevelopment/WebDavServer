// <copyright file="InMemoryFile.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Live;

namespace FubarDev.WebDavServer.FileSystem.InMemory
{
    public class InMemoryFile : InMemoryEntry, IDocument
    {
        private MemoryStream _data;

        public InMemoryFile(InMemoryFileSystem fileSystem, InMemoryDirectory parent, Uri path, string name)
            : this(fileSystem, parent, path, name, new byte[0])
        {
        }

        public InMemoryFile(InMemoryFileSystem fileSystem, InMemoryDirectory parent, Uri path, string name, byte[] data)
            : base(fileSystem, parent, path, name)
        {
            _data = new MemoryStream(data);
        }

        public long Length => _data.Length;

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

        public Task<Stream> OpenReadAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<Stream>(new MemoryStream(_data.ToArray()));
        }

        public Task<Stream> CreateAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<Stream>(_data = new MyMemoryStream(this));
        }

        public async Task<IDocument> CopyToAsync(ICollection collection, string name, CancellationToken cancellationToken)
        {
            var coll = (InMemoryDirectory)collection;
            coll.Remove(name);

            var doc = (InMemoryFile)await coll.CreateDocumentAsync(name, cancellationToken).ConfigureAwait(false);
            doc._data = new MemoryStream(_data.ToArray());
            doc.CreationTimeUtc = CreationTimeUtc;
            doc.LastWriteTimeUtc = LastWriteTimeUtc;
            doc.ETag = ETag;

            var sourcePropStore = FileSystem.PropertyStore;
            var destPropStore = collection.FileSystem.PropertyStore;
            if (sourcePropStore != null && destPropStore != null)
            {
                var sourceProps = await sourcePropStore.GetAsync(this, cancellationToken).ConfigureAwait(false);
                await destPropStore.RemoveAsync(doc, cancellationToken).ConfigureAwait(false);
                await destPropStore.SetAsync(doc, sourceProps, cancellationToken).ConfigureAwait(false);
            }
            else if (destPropStore != null)
            {
                await destPropStore.RemoveAsync(doc, cancellationToken).ConfigureAwait(false);
            }

            return doc;
        }

        public async Task<IDocument> MoveToAsync(ICollection collection, string name, CancellationToken cancellationToken)
        {
            var sourcePropStore = FileSystem.PropertyStore;
            var destPropStore = collection.FileSystem.PropertyStore;

            IReadOnlyCollection<XElement> sourceProps;
            if (sourcePropStore != null && destPropStore != null)
            {
                sourceProps = await sourcePropStore.GetAsync(this, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                sourceProps = null;
            }

            var coll = (InMemoryDirectory)collection;
            var doc = (InMemoryFile)await coll.CreateDocumentAsync(name, cancellationToken).ConfigureAwait(false);
            doc._data = new MemoryStream(_data.ToArray());
            doc.CreationTimeUtc = CreationTimeUtc;
            doc.LastWriteTimeUtc = LastWriteTimeUtc;
            doc.ETag = ETag;
            if (!InMemoryParent.Remove(Name))
                throw new InvalidOperationException("Failed to remove the document from the source collection.");

            if (destPropStore != null)
            {
                await destPropStore.RemoveAsync(doc, cancellationToken).ConfigureAwait(false);

                if (sourceProps != null)
                {
                    await destPropStore.SetAsync(doc, sourceProps, cancellationToken).ConfigureAwait(false);
                }
            }

            return doc;
        }

        protected override IEnumerable<ILiveProperty> GetLiveProperties()
        {
            return base.GetLiveProperties()
                       .Concat(new ILiveProperty[]
                       {
                           new ContentLengthProperty(ct => Task.FromResult(Length)),
                       });
        }

        protected override IEnumerable<IDeadProperty> GetPredefinedDeadProperties()
        {
            return base.GetPredefinedDeadProperties()
                .Concat(new[]
                {
                    InMemoryFileSystem.DeadPropertyFactory
                        .Create(FileSystem.PropertyStore, this, GetContentLanguageProperty.PropertyName),
                    InMemoryFileSystem.DeadPropertyFactory
                        .Create(FileSystem.PropertyStore, this, GetContentTypeProperty.PropertyName),
                });
        }

        private class MyMemoryStream : MemoryStream
        {
            private readonly InMemoryFile _file;

            public MyMemoryStream(InMemoryFile file)
            {
                _file = file;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _file._data = new MemoryStream(ToArray());
                }

                base.Dispose(disposing);
            }
        }
    }
}
