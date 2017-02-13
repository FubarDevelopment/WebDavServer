// <copyright file="InMemoryFileSystemFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Security.Principal;

using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Store;

namespace FubarDev.WebDavServer.FileSystem.InMemory
{
    public class InMemoryFileSystemFactory : IFileSystemFactory
    {
        private readonly Dictionary<string, InMemoryFileSystem> _fileSystems = new Dictionary<string, InMemoryFileSystem>(StringComparer.OrdinalIgnoreCase);
        private readonly PathTraversalEngine _pathTraversalEngine;
        private readonly ISystemClock _systemClock;
        private readonly IDeadPropertyFactory _deadPropertyFactory;
        private readonly IPropertyStoreFactory _propertyStoreFactory;

        public InMemoryFileSystemFactory(PathTraversalEngine pathTraversalEngine, ISystemClock systemClock, IDeadPropertyFactory deadPropertyFactory, IPropertyStoreFactory propertyStoreFactory = null)
        {
            _pathTraversalEngine = pathTraversalEngine;
            _systemClock = systemClock;
            _deadPropertyFactory = deadPropertyFactory;
            _propertyStoreFactory = propertyStoreFactory;
        }

        public IFileSystem CreateFileSystem(IIdentity identity)
        {
            var userName = identity.IsAuthenticated ? identity.Name : string.Empty;

            InMemoryFileSystem fileSystem;
            if (!_fileSystems.TryGetValue(userName, out fileSystem))
            {
                fileSystem = new InMemoryFileSystem(_pathTraversalEngine, _systemClock, _deadPropertyFactory, _propertyStoreFactory);
                _fileSystems.Add(userName, fileSystem);
            }

            return fileSystem;
        }
    }
}
