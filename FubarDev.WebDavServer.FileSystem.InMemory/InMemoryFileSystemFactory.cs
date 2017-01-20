using System;
using System.Collections.Generic;
using System.Security.Principal;

using FubarDev.WebDavServer.Properties;

namespace FubarDev.WebDavServer.FileSystem.InMemory
{
    public class InMemoryFileSystemFactory : IFileSystemFactory
    {
        private readonly Dictionary<string, InMemoryFileSystem> _fileSystems = new Dictionary<string, InMemoryFileSystem>(StringComparer.OrdinalIgnoreCase);
        private readonly PathTraversalEngine _pathTraversalEngine;
        private readonly IPropertyStore _propertyStore;

        public InMemoryFileSystemFactory(PathTraversalEngine pathTraversalEngine, IPropertyStore propertyStore)
        {
            _pathTraversalEngine = pathTraversalEngine;
            _propertyStore = propertyStore;
        }

        public IFileSystem CreateFileSystem(IIdentity identity)
        {
            var userName = identity.IsAuthenticated ? identity.Name : string.Empty;

            InMemoryFileSystem fileSystem;
            if (!_fileSystems.TryGetValue(userName, out fileSystem))
            {
                fileSystem = new InMemoryFileSystem(_pathTraversalEngine, _propertyStore);
                _fileSystems.Add(userName, fileSystem);
            }

            return fileSystem;
        }
    }
}
