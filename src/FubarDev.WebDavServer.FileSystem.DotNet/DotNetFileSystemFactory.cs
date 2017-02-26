// <copyright file="DotNetFileSystemFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Security.Principal;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Store;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    /// <summary>
    /// The factory creating/getting the file systems that use <see cref="System.IO"/> for its implementation
    /// </summary>
    public class DotNetFileSystemFactory : IFileSystemFactory
    {
        [NotNull]
        private readonly PathTraversalEngine _pathTraversalEngine;

        [NotNull]
        private readonly IDeadPropertyFactory _deadPropertyFactory;

        [CanBeNull]
        private readonly IPropertyStoreFactory _propertyStoreFactory;

        [CanBeNull]
        private readonly ILockManager _lockManager;

        [NotNull]
        private readonly DotNetFileSystemOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetFileSystemFactory"/> class.
        /// </summary>
        /// <param name="options">The options for this file system</param>
        /// <param name="pathTraversalEngine">The engine to traverse paths</param>
        /// <param name="deadPropertyFactory">A factory for dead properties</param>
        /// <param name="propertyStoreFactory">The store for dead properties</param>
        /// <param name="lockManager">The global lock manager</param>
        public DotNetFileSystemFactory(IOptions<DotNetFileSystemOptions> options, PathTraversalEngine pathTraversalEngine, IDeadPropertyFactory deadPropertyFactory, IPropertyStoreFactory propertyStoreFactory = null, ILockManager lockManager = null)
        {
            _pathTraversalEngine = pathTraversalEngine;
            _deadPropertyFactory = deadPropertyFactory;
            _propertyStoreFactory = propertyStoreFactory;
            _lockManager = lockManager;
            _options = options.Value;
        }

        /// <inheritdoc />
        public IFileSystem CreateFileSystem(IIdentity identity)
        {
            var userHomeDirectory = Path.Combine(_options.RootPath, identity.IsAuthenticated ? identity.Name : _options.AnonymousUserName);
            Directory.CreateDirectory(userHomeDirectory);
            return new DotNetFileSystem(_options, userHomeDirectory, _pathTraversalEngine, _deadPropertyFactory, _lockManager, _propertyStoreFactory);
        }
    }
}
