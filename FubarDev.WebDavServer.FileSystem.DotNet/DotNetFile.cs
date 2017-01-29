using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties.Dead;
using FubarDev.WebDavServer.Properties.Live;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    public class DotNetFile : DotNetEntry, IDocument
    {
        public DotNetFile(DotNetFileSystem fileSystem, DotNetDirectory parent, FileInfo info, Uri path)
            : base(fileSystem, parent, info, path)
        {
            FileInfo = info;
        }

        public FileInfo FileInfo { get; }

        public long Length => FileInfo.Length;

        public Task<Stream> OpenReadAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<Stream>(FileInfo.OpenRead());
        }

        public Task<Stream> CreateAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<Stream>(FileInfo.Open(FileMode.Create, FileAccess.Write));
        }

        public override Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken)
        {
            FileInfo.Delete();
            return Task.FromResult(new DeleteResult(WebDavStatusCode.OK, null));
        }

        public Task<IDocument> CopyToAsync(ICollection collection, string name, CancellationToken cancellationToken)
        {
            var dir = (DotNetDirectory) collection;
            var targetFileName = System.IO.Path.Combine(dir.DirectoryInfo.FullName, name);
            File.Copy(FileInfo.FullName, targetFileName, true);
            var fileInfo = new FileInfo(targetFileName);
            var result = new DotNetFile(dir.DotNetFileSystem, dir, fileInfo, dir.Path.Append(fileInfo.Name, false));
            return Task.FromResult<IDocument>(result);
        }

        public Task<IDocument> MoveToAsync(ICollection collection, string name, CancellationToken cancellationToken)
        {
            var dir = (DotNetDirectory)collection;
            var targetFileName = System.IO.Path.Combine(dir.DirectoryInfo.FullName, name);
            if (File.Exists(targetFileName))
                File.Delete(targetFileName);
            File.Move(FileInfo.FullName, targetFileName);
            var fileInfo = new FileInfo(targetFileName);
            var result = new DotNetFile(dir.DotNetFileSystem, dir, fileInfo, dir.Path.Append(fileInfo.Name, false));
            return Task.FromResult<IDocument>(result);
        }

        protected override IEnumerable<ILiveProperty> GetLiveProperties()
        {
            foreach (var property in base.GetLiveProperties())
            {
                yield return property;
            }

            yield return new ContentLengthProperty(ct => Task.FromResult(Length));
        }

        protected override IEnumerable<IDeadProperty> GetPredefinedDeadProperties()
        {
            foreach (var property in base.GetPredefinedDeadProperties())
            {
                yield return property;
            }

            yield return new GetETagProperty(FileSystem.PropertyStore, this, 0);
        }
    }
}
