// <copyright file="InMemoryFileSystemFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Props.Store;
using FubarDev.WebDavServer.Utils;

namespace FubarDev.WebDavServer.FileSystem.InMemory
{
    /// <summary>
    /// An in-memory implementation of the <see cref="IFileSystemFactory"/>.
    /// </summary>
    public class InMemoryFileSystemFactory : IFileSystemFactory
    {
        private readonly Dictionary<FileSystemKey, InMemoryFileSystem> _fileSystems = new Dictionary<FileSystemKey, InMemoryFileSystem>();
        private readonly IPathTraversalEngine _pathTraversalEngine;
        private readonly ILockManager? _lockManager;
        private readonly IPropertyStoreFactory? _propertyStoreFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryFileSystemFactory"/> class.
        /// </summary>
        /// <param name="pathTraversalEngine">The engine to traverse paths.</param>
        /// <param name="lockManager">The global lock manager.</param>
        /// <param name="propertyStoreFactory">The store for dead properties.</param>
        public InMemoryFileSystemFactory(
            IPathTraversalEngine pathTraversalEngine,
            ILockManager? lockManager = null,
            IPropertyStoreFactory? propertyStoreFactory = null)
        {
            _pathTraversalEngine = pathTraversalEngine;
            _lockManager = lockManager;
            _propertyStoreFactory = propertyStoreFactory;
        }

        /// <inheritdoc />
        public virtual IFileSystem CreateFileSystem(ICollection? mountPoint, IPrincipal principal)
        {
            var userName = !principal.Identity.IsAnonymous()
                ? principal.Identity.Name
                : SystemInfo.GetAnonymousUserName();

            var key = new FileSystemKey(userName, mountPoint?.Path.OriginalString ?? string.Empty);
            if (!_fileSystems.TryGetValue(key, out var fileSystem))
            {
                fileSystem = new InMemoryFileSystem(mountPoint, _pathTraversalEngine, _lockManager, _propertyStoreFactory);
                _fileSystems.Add(key, fileSystem);
                InitializeFileSystem(mountPoint, principal, fileSystem);
            }
            else
            {
                UpdateFileSystem(mountPoint, principal, fileSystem);
            }

            return fileSystem;
        }

        /// <summary>
        /// Called when file system will be initialized.
        /// </summary>
        /// <param name="mountPoint">The mount point.</param>
        /// <param name="principal">The principal the file system was created for.</param>
        /// <param name="fileSystem">The created file system.</param>
        protected virtual void InitializeFileSystem(ICollection? mountPoint, IPrincipal principal, InMemoryFileSystem fileSystem)
        {
        }

        /// <summary>
        /// Called when the file system will be updated.
        /// </summary>
        /// <param name="mountPoint">The mount point.</param>
        /// <param name="principal">The principal the file system was created for.</param>
        /// <param name="fileSystem">The created file system.</param>
        protected virtual void UpdateFileSystem(ICollection? mountPoint, IPrincipal principal, InMemoryFileSystem fileSystem)
        {
        }

        [SuppressMessage(
            "ReSharper",
            "NotAccessedPositionalProperty.Local",
            Justification = "Just used as dictionary key")]
        private record FileSystemKey(string UserName, string MountPoint);
    }
}
