using System;
using System.IO;
using System.Security.Principal;
using System.Threading;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.DotNet;
using FubarDev.WebDavServer.Properties.Store;
using FubarDev.WebDavServer.Properties.Store.TextFile;

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
            var propertyStore = _serviceProvider.GetRequiredService<IPropertyStore>();
            var fileSystemPropStore = propertyStore as IFileSystemPropertyStore;
            if (fileSystemPropStore != null)
                fileSystemPropStore.RootPath = userHomeDirectory;
            return new DotNetFileSystem(_options, propertyStore, userHomeDirectory);
        }
    }
}
