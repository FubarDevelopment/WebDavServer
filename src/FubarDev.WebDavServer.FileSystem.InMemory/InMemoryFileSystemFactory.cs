// <copyright file="InMemoryFileSystemFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Security.Principal;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Store;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem.InMemory
{
    /// <summary>
    /// An in-memory implementation of the <see cref="IFileSystemFactory"/>
    /// </summary>
    public class InMemoryFileSystemFactory : IFileSystemFactory
    {
        [NotNull]
        private readonly Dictionary<string, InMemoryFileSystem> _fileSystems = new Dictionary<string, InMemoryFileSystem>(StringComparer.OrdinalIgnoreCase);

        [NotNull]
        private readonly PathTraversalEngine _pathTraversalEngine;

        [NotNull]
        private readonly ISystemClock _systemClock;

        [NotNull]
        private readonly IDeadPropertyFactory _deadPropertyFactory;

        [CanBeNull]
        private readonly ILockManager _lockManager;

        [CanBeNull]
        private readonly IPropertyStoreFactory _propertyStoreFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryFileSystemFactory"/> class.
        /// </summary>
        /// <param name="pathTraversalEngine">The engine to traverse paths</param>
        /// <param name="systemClock">Interface for the access to the systems clock</param>
        /// <param name="deadPropertyFactory">A factory for dead properties</param>
        /// <param name="lockManager">The global lock manager</param>
        /// <param name="propertyStoreFactory">The store for dead properties</param>
        public InMemoryFileSystemFactory(PathTraversalEngine pathTraversalEngine, ISystemClock systemClock, IDeadPropertyFactory deadPropertyFactory, ILockManager lockManager = null, IPropertyStoreFactory propertyStoreFactory = null)
        {
            _pathTraversalEngine = pathTraversalEngine;
            _systemClock = systemClock;
            _deadPropertyFactory = deadPropertyFactory;
            _lockManager = lockManager;
            _propertyStoreFactory = propertyStoreFactory;
        }

        /// <inheritdoc />
        public IFileSystem CreateFileSystem(IPrincipal principal)
        {
            var userName = principal.Identity.IsAuthenticated ? principal.Identity.Name : string.Empty;

            InMemoryFileSystem fileSystem;
            if (!_fileSystems.TryGetValue(userName, out fileSystem))
            {
                fileSystem = new InMemoryFileSystem(_pathTraversalEngine, _systemClock, _deadPropertyFactory, _lockManager, _propertyStoreFactory);
                _fileSystems.Add(userName, fileSystem);
            }

            return fileSystem;
        }
    }
}
