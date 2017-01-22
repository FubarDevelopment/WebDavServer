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

        public Task<IDocument> CopyToAsync(ICollection collection, string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IDocument> MoveToAsync(ICollection collection, string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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
    }
}
