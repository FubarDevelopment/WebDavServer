using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Properties;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    public class DotNetFile : DotNetEntry, IDocument
    {
        public DotNetFile(DotNetFileSystem fileSystem, FileInfo info, string path)
            : base(fileSystem, info, path)
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

        protected override IEnumerable<IProperty> CreateProperties()
        {
            foreach (var property in base.CreateProperties())
            {
                yield return property;
            }

            yield return new ContentLength(ct => Task.FromResult(Length));
        }
    }
}
