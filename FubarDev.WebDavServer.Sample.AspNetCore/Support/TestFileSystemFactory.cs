using System;
using System.IO;
using System.Security.Principal;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.DotNet;
using FubarDev.WebDavServer.Properties;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Sample.AspNetCore.Support
{
    public class TestFileSystemFactory : IFileSystemFactory
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly DotNetFileSystemOptions _options;

        public TestFileSystemFactory(IOptions<DotNetFileSystemOptions> options, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }

        public IFileSystem CreateFileSystem(IIdentity identity)
        {
            var userHomeDirectory = Path.Combine(_options.RootPath, identity.IsAuthenticated ? identity.Name : _options.AnonymousUserName);
            var pathTraversalEngine = _serviceProvider.GetRequiredService<PathTraversalEngine>();
            var propertyStore = _serviceProvider.GetService<IPropertyStore>();
            var fileSystemPropStore = propertyStore as IFileSystemPropertyStore;
            if (fileSystemPropStore != null)
                fileSystemPropStore.RootPath = userHomeDirectory;
            Directory.CreateDirectory(userHomeDirectory);
            return new DotNetFileSystem(_options, userHomeDirectory, pathTraversalEngine, propertyStore);
        }
    }
}
