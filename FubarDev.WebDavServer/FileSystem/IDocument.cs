using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface IDocument : IEntry
    {
        long Length { get; }
        Task<Stream> OpenReadAsync(CancellationToken cancellationToken);
        Task<Stream> CreateAsync(CancellationToken cancellationToken);
    }
}
