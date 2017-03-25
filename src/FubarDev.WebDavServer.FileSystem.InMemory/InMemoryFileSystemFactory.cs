// <copyright file="InMemoryFileSystemFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Security.Principal;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Store;
using FubarDev.WebDavServer.Utils;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

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

        [NotNull]
        private readonly InMemoryFileSystemOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryFileSystemFactory"/> class.
        /// </summary>
        /// <param name="options">The options for the in-memory file system</param>
        /// <param name="pathTraversalEngine">The engine to traverse paths</param>
        /// <param name="systemClock">Interface for the access to the systems clock</param>
        /// <param name="deadPropertyFactory">A factory for dead properties</param>
        /// <param name="lockManager">The global lock manager</param>
        /// <param name="propertyStoreFactory">The store for dead properties</param>
        public InMemoryFileSystemFactory(IOptions<InMemoryFileSystemOptions> options, PathTraversalEngine pathTraversalEngine, ISystemClock systemClock, IDeadPropertyFactory deadPropertyFactory, ILockManager lockManager = null, IPropertyStoreFactory propertyStoreFactory = null)
        {
            _options = options.Value ?? new InMemoryFileSystemOptions();
            _pathTraversalEngine = pathTraversalEngine;
            _systemClock = systemClock;
            _deadPropertyFactory = deadPropertyFactory;
            _lockManager = lockManager;
            _propertyStoreFactory = propertyStoreFactory;
        }

        /// <inheritdoc />
        public IFileSystem CreateFileSystem(ICollection mountPoint, IPrincipal principal)
        {
            var userName = !principal.Identity.IsAnonymous()
                ? principal.Identity.Name
                : string.Empty;

            InMemoryFileSystem fileSystem;
            if (!_fileSystems.TryGetValue(userName, out fileSystem))
            {
                fileSystem = new InMemoryFileSystem(mountPoint, _pathTraversalEngine, _systemClock, _deadPropertyFactory, _lockManager, _propertyStoreFactory);
                var eventArgs = new InMemoryFileSystemInitializationEventArgs(fileSystem, principal);
                _options.OnInitialize(this, eventArgs);
                fileSystem.IsReadOnly = eventArgs.IsReadOnly;
                _fileSystems.Add(userName, fileSystem);
            }
            else
            {
                fileSystem.IsReadOnly = false;
                var eventArgs = new InMemoryFileSystemInitializationEventArgs(fileSystem, principal);
                _options.OnUpdate(this, eventArgs);
                fileSystem.IsReadOnly = eventArgs.IsReadOnly;
            }

            return fileSystem;
        }
    }
}
