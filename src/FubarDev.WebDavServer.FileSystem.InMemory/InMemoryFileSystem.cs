// <copyright file="InMemoryFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Store;

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
        /// <param name="pathTraversalEngine">The engine to traverse paths</param>
        /// <param name="systemClock">Interface for the access to the systems clock</param>
        /// <param name="deadPropertyFactory">A factory for dead properties</param>
        /// <param name="lockManager">The global lock manager</param>
        /// <param name="propertyStoreFactory">The store for dead properties</param>
        public InMemoryFileSystem(PathTraversalEngine pathTraversalEngine, ISystemClock systemClock, IDeadPropertyFactory deadPropertyFactory, ILockManager lockManager = null, IPropertyStoreFactory propertyStoreFactory = null)
        {
            SystemClock = systemClock;
            DeadPropertyFactory = deadPropertyFactory;
            LockManager = lockManager;
            _pathTraversalEngine = pathTraversalEngine;
            var rootDir = new InMemoryDirectory(this, null, new Uri(string.Empty, UriKind.Relative), string.Empty);
            Root = new AsyncLazy<ICollection>(() => Task.FromResult<ICollection>(rootDir));
            PropertyStore = propertyStoreFactory?.Create(this);
        }

        /// <summary>
        /// Gets the systems clock
        /// </summary>
        public ISystemClock SystemClock { get; }

        /// <summary>
        /// Gets the factory for dead properties
        /// </summary>
        public IDeadPropertyFactory DeadPropertyFactory { get; }

        /// <inheritdoc />
        public AsyncLazy<ICollection> Root { get; }

        /// <inheritdoc />
        public IPropertyStore PropertyStore { get; }

        /// <inheritdoc />
        public ILockManager LockManager { get; }

        /// <inheritdoc />
        public Task<SelectionResult> SelectAsync(string path, CancellationToken ct)
        {
            return _pathTraversalEngine.TraverseAsync(this, path, ct);
        }
    }
}
