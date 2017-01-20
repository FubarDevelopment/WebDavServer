using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties;

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
                ? new DeleteResult(WebDavStatusCodes.NotFound, this)
                : new DeleteResult(WebDavStatusCodes.OK, null);
            return Task.FromResult(result);
        }

        public Task<Stream> OpenReadAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<Stream>(new MemoryStream(_data.ToArray()));
        }

        public Task<Stream> CreateAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<Stream>(_data = new MemoryStream());
        }

        public Task<IEntry> CopyToAsync(ICollection collection, string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEntry> MoveToAsync(ICollection collection, string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<IUntypedReadableProperty> GetLiveProperties()
        {
            foreach (var liveProperty in base.GetLiveProperties())
            {
                yield return liveProperty;
            }

            yield return new ContentLengthProperty(ct => Task.FromResult(Length));
            yield return new GetETagProperty(FileSystem.PropertyStore, this, 0);
        }
    }
}
