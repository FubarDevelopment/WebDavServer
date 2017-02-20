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
    public class InMemoryFileSystem : IFileSystem
    {
        private readonly PathTraversalEngine _pathTraversalEngine;

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

        public ISystemClock SystemClock { get; }

        public IDeadPropertyFactory DeadPropertyFactory { get; }

        public AsyncLazy<ICollection> Root { get; }

        public IPropertyStore PropertyStore { get; }

        public ILockManager LockManager { get; }

        public Task<SelectionResult> SelectAsync(string path, CancellationToken ct)
        {
            return _pathTraversalEngine.TraverseAsync(this, path, ct);
        }
    }
}
