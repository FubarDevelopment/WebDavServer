using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface IFileSystemFactory
    {
        Task<IFileSystem> CreateFileSystemAsync(string userName, bool isAnonymous, CancellationToken ct);
    }
}
