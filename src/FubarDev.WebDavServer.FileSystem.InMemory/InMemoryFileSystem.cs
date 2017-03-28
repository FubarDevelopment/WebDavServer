// <copyright file="InMemoryFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem.Mount;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Props.Store;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem.InMemory
{
    /// <summary>
    /// An in-memory file system implementation
    /// </summary>
    public class InMemoryFileSystem : IFileSystem, IMountPointManager
    {
        private readonly IPathTraversalEngine _pathTraversalEngine;

        private readonly Dictionary<Uri, IFileSystem> _mountPoints = new Dictionary<Uri, IFileSystem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryFileSystem"/> class.
        /// </summary>
        /// <param name="mountPoint">The mount point where this file system should be included</param>
        /// <param name="pathTraversalEngine">The engine to traverse paths</param>
        /// <param name="systemClock">Interface for the access to the systems clock</param>
        /// <param name="lockManager">The global lock manager</param>
        /// <param name="propertyStoreFactory">The store for dead properties</param>
        public InMemoryFileSystem(
            [CanBeNull] ICollection mountPoint,
            [NotNull] IPathTraversalEngine pathTraversalEngine,
            [NotNull] ISystemClock systemClock,
            ILockManager lockManager = null,
            IPropertyStoreFactory propertyStoreFactory = null)
        {
            SystemClock = systemClock;
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
        public IEnumerable<Uri> MountPoints => _mountPoints.Keys;

        /// <inheritdoc />
        public Task<SelectionResult> SelectAsync(string path, CancellationToken ct)
        {
            return _pathTraversalEngine.TraverseAsync(this, path, ct);
        }

        /// <inheritdoc />
        public bool TryGetMountPoint(Uri path, out IFileSystem destination)
        {
            return _mountPoints.TryGetValue(path, out destination);
        }

        /// <inheritdoc />
        public void Mount(Uri source, IFileSystem destination)
        {
            _mountPoints.Add(source, destination);
        }

        /// <inheritdoc />
        public void Unmount(Uri source)
        {
            _mountPoints.Remove(source);
        }
    }
}
