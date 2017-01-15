using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface IFileSystemFactory
    {
        IFileSystem CreateFileSystem(IIdentity identity);
    }
}
