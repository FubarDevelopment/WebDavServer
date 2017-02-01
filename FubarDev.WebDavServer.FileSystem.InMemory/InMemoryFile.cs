using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties.Dead;
using FubarDev.WebDavServer.Properties.Live;

namespace FubarDev.WebDavServer.FileSystem.InMemory
{
    public class InMemoryFile : InMemoryEntry, IDocument
    {
        private MemoryStream _data;

        public InMemoryFile(IFileSystem fileSystem, InMemoryDirectory parent, Uri path, string name)
            : this(fileSystem, parent, path, name, new byte[0])
        {
        }

        public InMemoryFile(IFileSystem fileSystem, InMemoryDirectory parent, Uri path, string name, byte[] data)
            : base(fileSystem, parent, path, name)
        {
            _data = new MemoryStream(data);
        }

        public long Length => _data.Length;

        public override Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken)
        {
            var result = !InMemoryParent.Remove(Name)
                ? new DeleteResult(WebDavStatusCode.NotFound, this)
                : new DeleteResult(WebDavStatusCode.OK, null);
            return Task.FromResult(result);
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
            var coll = (InMemoryDirectory) collection;
            var doc = (InMemoryFile)await coll.CreateDocumentAsync(name, cancellationToken).ConfigureAwait(false);
            doc._data = new MemoryStream(_data.ToArray());
            doc.CreationTimeUtc = CreationTimeUtc;
            doc.LastWriteTimeUtc = LastWriteTimeUtc;
            return doc;
        }

        public async Task<IDocument> MoveToAsync(ICollection collection, string name, CancellationToken cancellationToken)
        {
            var coll = (InMemoryDirectory)collection;
            var doc = (InMemoryFile)await coll.CreateDocumentAsync(name, cancellationToken).ConfigureAwait(false);
            doc._data = new MemoryStream(_data.ToArray());
            doc.CreationTimeUtc = CreationTimeUtc;
            doc.LastWriteTimeUtc = LastWriteTimeUtc;
            if (!InMemoryParent.Remove(name))
                throw new InvalidOperationException("Failed to remove the document from the source collection.");
            return doc;
        }

        protected override IEnumerable<ILiveProperty> GetLiveProperties()
        {
            return base.GetLiveProperties()
                       .Concat(new ILiveProperty[]
                       {
                           new ContentLengthProperty(ct => Task.FromResult(Length))
                       });
        }

        protected override IEnumerable<IDeadProperty> GetPredefinedDeadProperties()
        {
            return base.GetPredefinedDeadProperties()
                       .Concat(new IDeadProperty[]
                       {
                           new GetETagProperty(FileSystem.PropertyStore, this, 0)
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
