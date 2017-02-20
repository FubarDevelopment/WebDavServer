// <copyright file="DotNetFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Store;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    public class DotNetFileSystem : ILocalFileSystem
    {
        private readonly PathTraversalEngine _pathTraversalEngine;

        public DotNetFileSystem(DotNetFileSystemOptions options, string rootFolder, PathTraversalEngine pathTraversalEngine, IDeadPropertyFactory deadPropertyFactory, ILockManager lockManager = null, IPropertyStoreFactory propertyStoreFactory = null)
        {
            LockManager = lockManager;
            RootDirectoryPath = rootFolder;
            DeadPropertyFactory = deadPropertyFactory;
            _pathTraversalEngine = pathTraversalEngine;
            Options = options;
            PropertyStore = propertyStoreFactory?.Create(this);
            var rootDir = new DotNetDirectory(this, null, new DirectoryInfo(rootFolder), new Uri(string.Empty, UriKind.Relative));
            Root = new AsyncLazy<ICollection>(() => Task.FromResult<ICollection>(rootDir));
        }

        public string RootDirectoryPath { get; }

        public IDeadPropertyFactory DeadPropertyFactory { get; }

        /// <inheritdoc />
        public AsyncLazy<ICollection> Root { get; }

        public DotNetFileSystemOptions Options { get; }

        /// <inheritdoc />
        public IPropertyStore PropertyStore { get; }

        /// <inheritdoc />
        public ILockManager LockManager { get; }

        public Task<SelectionResult> SelectAsync(string path, CancellationToken ct)
        {
            return _pathTraversalEngine.TraverseAsync(this, path, ct);
        }
    }
}
