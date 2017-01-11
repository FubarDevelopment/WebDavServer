using System.IO;
using System.Security.Principal;
using System.Threading;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.DotNet;

namespace FubarDev.WebDavServer.Sample.AspNetCore.Support
{
    public class TestFileSystemFactory : IFileSystemFactory
    {
        private readonly string _rootPath;

        public TestFileSystemFactory(string rootPath)
        {
            _rootPath = rootPath;
        }

        public IFileSystem CreateFileSystem(IIdentity identity)
        {
            var userHomeDirectory = Path.Combine(_rootPath, identity.IsAuthenticated ? identity.Name : "Public");
            return new DotNetFileSystem(userHomeDirectory);
        }
    }
}
