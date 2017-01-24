using System.IO;
using System.Security.Principal;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.DotNet;
using FubarDev.WebDavServer.Properties.Store;

using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Sample.AspNetCore.Support
{
    public class TestFileSystemFactory : IFileSystemFactory
    {
        private readonly PathTraversalEngine _pathTraversalEngine;
        private readonly IPropertyStoreFactory _propertyStoreFactory;
        private readonly DotNetFileSystemOptions _options;

        public TestFileSystemFactory(IOptions<DotNetFileSystemOptions> options, PathTraversalEngine pathTraversalEngine, IPropertyStoreFactory propertyStoreFactory)
        {
            _pathTraversalEngine = pathTraversalEngine;
            _propertyStoreFactory = propertyStoreFactory;
            _options = options.Value;
        }

        public IFileSystem CreateFileSystem(IIdentity identity)
        {
            var userHomeDirectory = Path.Combine(_options.RootPath, identity.IsAuthenticated ? identity.Name : _options.AnonymousUserName);
            Directory.CreateDirectory(userHomeDirectory);
            return new DotNetFileSystem(_options, userHomeDirectory, _pathTraversalEngine, _propertyStoreFactory);
        }
    }
}
