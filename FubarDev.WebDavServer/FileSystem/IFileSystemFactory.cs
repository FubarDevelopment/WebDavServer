using System.Security.Principal;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface IFileSystemFactory
    {
        IFileSystem CreateFileSystem(IIdentity identity);
    }
}
