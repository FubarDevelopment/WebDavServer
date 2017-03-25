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
    /// <summary>
    /// A file system implementation using <see cref="System.IO"/>
    /// </summary>
    public class DotNetFileSystem : ILocalFileSystem
    {
        private readonly PathTraversalEngine _pathTraversalEngine;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetFileSystem"/> class.
        /// </summary>
        /// <param name="options">The options for this file system</param>
        /// <param name="rootPath">The path of the root collection</param>
        /// <param name="rootFolder">The root folder</param>
        /// <param name="pathTraversalEngine">The engine to traverse paths</param>
        /// <param name="deadPropertyFactory">A factory for dead properties</param>
        /// <param name="lockManager">The global lock manager</param>
        /// <param name="propertyStoreFactory">The store for dead properties</param>
        public DotNetFileSystem(DotNetFileSystemOptions options, Uri rootPath, string rootFolder, PathTraversalEngine pathTraversalEngine, IDeadPropertyFactory deadPropertyFactory, ILockManager lockManager = null, IPropertyStoreFactory propertyStoreFactory = null)
        {
            LockManager = lockManager;
            RootDirectoryPath = rootFolder;
            DeadPropertyFactory = deadPropertyFactory;
            _pathTraversalEngine = pathTraversalEngine;
            Options = options;
            PropertyStore = propertyStoreFactory?.Create(this);
            var rootDir = new DotNetDirectory(this, null, new DirectoryInfo(rootFolder), rootPath);
            Root = new AsyncLazy<ICollection>(() => Task.FromResult<ICollection>(rootDir));
        }

        /// <inheritdoc />
        public string RootDirectoryPath { get; }

        /// <inheritdoc />
        public bool HasSubfolders { get; } = true;

        /// <summary>
        /// Gets the factory for dead properties
        /// </summary>
        public IDeadPropertyFactory DeadPropertyFactory { get; }

        /// <inheritdoc />
        public AsyncLazy<ICollection> Root { get; }

        /// <summary>
        /// Gets the file systems options
        /// </summary>
        public DotNetFileSystemOptions Options { get; }

        /// <inheritdoc />
        public IPropertyStore PropertyStore { get; }

        /// <inheritdoc />
        public ILockManager LockManager { get; }

        /// <inheritdoc />
        public bool SupportsRangedRead { get; } = true;

        /// <inheritdoc />
        public Task<SelectionResult> SelectAsync(string path, CancellationToken ct)
        {
            return _pathTraversalEngine.TraverseAsync(this, path, ct);
        }
    }
}
