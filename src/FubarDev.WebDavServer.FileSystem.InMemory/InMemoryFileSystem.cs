// <copyright file="InMemoryFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem.Mount;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Store;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem.InMemory
{
    /// <summary>
    /// An in-memory file system implementation
    /// </summary>
    public class InMemoryFileSystem : IFileSystem
    {
        private readonly PathTraversalEngine _pathTraversalEngine;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryFileSystem"/> class.
        /// </summary>
        /// <param name="mountPoint">The mount point where this file system should be included</param>
        /// <param name="pathTraversalEngine">The engine to traverse paths</param>
        /// <param name="systemClock">Interface for the access to the systems clock</param>
        /// <param name="deadPropertyFactory">A factory for dead properties</param>
        /// <param name="mountPointProvider">The mount point provider</param>
        /// <param name="lockManager">The global lock manager</param>
        /// <param name="propertyStoreFactory">The store for dead properties</param>
        public InMemoryFileSystem(
            [CanBeNull] ICollection mountPoint,
            [NotNull] PathTraversalEngine pathTraversalEngine,
            [NotNull] ISystemClock systemClock,
            [NotNull] IDeadPropertyFactory deadPropertyFactory,
            [NotNull] IMountPointProvider mountPointProvider,
            ILockManager lockManager = null,
            IPropertyStoreFactory propertyStoreFactory = null)
        {
            SystemClock = systemClock;
            DeadPropertyFactory = deadPropertyFactory;
            MountPointProvider = mountPointProvider;
            LockManager = lockManager;
            _pathTraversalEngine = pathTraversalEngine;
            var rootPath = mountPoint?.Path ?? new Uri(string.Empty, UriKind.Relative);
            RootCollection = new InMemoryDirectory(this, mountPoint, rootPath, mountPoint?.Name ?? rootPath.GetName(), true);
            Root = new AsyncLazy<ICollection>(() => Task.FromResult<ICollection>(RootCollection));
            PropertyStore = propertyStoreFactory?.Create(this);
        }

        /// <summary>
        /// Gets the root collection
        /// </summary>
        [NotNull]
        public InMemoryDirectory RootCollection { get; }

        /// <summary>
        /// Gets the systems clock
        /// </summary>
        public ISystemClock SystemClock { get; }

        /// <summary>
        /// Gets the factory for dead properties
        /// </summary>
        public IDeadPropertyFactory DeadPropertyFactory { get; }

        /// <summary>
        /// Gets the mount point manager
        /// </summary>
        [NotNull]
        public IMountPointProvider MountPointProvider { get; }

        /// <inheritdoc />
        public AsyncLazy<ICollection> Root { get; }

        /// <inheritdoc />
        public IPropertyStore PropertyStore { get; }

        /// <inheritdoc />
        public ILockManager LockManager { get; }

        /// <inheritdoc />
        public bool SupportsRangedRead { get; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the file system is read-only.
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <inheritdoc />
        public Task<SelectionResult> SelectAsync(string path, CancellationToken ct)
        {
            return _pathTraversalEngine.TraverseAsync(this, path, ct);
        }
    }
}
