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

        public DotNetFileSystemFactory(IOptions<DotNetFileSystemOptions> options, PathTraversalEngine pathTraversalEngine, IDeadPropertyFactory deadPropertyFactory, IPropertyStoreFactory propertyStoreFactory = null, ILockManager lockManager = null)
        {
            _pathTraversalEngine = pathTraversalEngine;
            _deadPropertyFactory = deadPropertyFactory;
            _propertyStoreFactory = propertyStoreFactory;
            _lockManager = lockManager;
            _options = options.Value;
        }

        public IFileSystem CreateFileSystem(IIdentity identity)
        {
            var userHomeDirectory = Path.Combine(_options.RootPath, identity.IsAuthenticated ? identity.Name : _options.AnonymousUserName);
            Directory.CreateDirectory(userHomeDirectory);
            return new DotNetFileSystem(_options, userHomeDirectory, _pathTraversalEngine, _deadPropertyFactory, _lockManager, _propertyStoreFactory);
        }
    }
}
