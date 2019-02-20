// <copyright file="DotNetFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem.Mount;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Props.Store;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    /// <summary>
    /// A file system implementation using <see cref="System.IO"/>.
    /// </summary>
    public class DotNetFileSystem : ILocalFileSystem, IMountPointManager
    {
        private readonly IPathTraversalEngine _pathTraversalEngine;

        private readonly Dictionary<Uri, IFileSystem> _mountPoints = new Dictionary<Uri, IFileSystem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetFileSystem"/> class.
        /// </summary>
        /// <param name="options">The options for this file system.</param>
        /// <param name="mountPoint">The mount point where this file system should be included.</param>
        /// <param name="rootFolder">The root folder.</param>
        /// <param name="pathTraversalEngine">The engine to traverse paths.</param>
        /// <param name="lockManager">The global lock manager.</param>
        /// <param name="propertyStoreFactory">The store for dead properties.</param>
        public DotNetFileSystem(
            [NotNull] DotNetFileSystemOptions options,
            [CanBeNull] ICollection mountPoint,
            [NotNull] string rootFolder,
            [NotNull] IPathTraversalEngine pathTraversalEngine,
            ILockManager lockManager = null,
            IPropertyStoreFactory propertyStoreFactory = null)
        {
            LockManager = lockManager;
            RootDirectoryPath = rootFolder;
            _pathTraversalEngine = pathTraversalEngine;
            Options = options;
            PropertyStore = propertyStoreFactory?.Create(this);
            var rootPath = mountPoint?.Path ?? new Uri(string.Empty, UriKind.Relative);
            var rootDir = new DotNetDirectory(this, mountPoint?.Parent, new DirectoryInfo(rootFolder), rootPath, mountPoint?.Name ?? rootPath.GetName(), true);
            Root = new AsyncLazy<ICollection>(() => Task.FromResult<ICollection>(rootDir));
        }

        /// <inheritdoc />
        public string RootDirectoryPath { get; }

        /// <inheritdoc />
        public bool HasSubfolders { get; } = true;

        /// <inheritdoc />
        public AsyncLazy<ICollection> Root { get; }

        /// <summary>
        /// Gets the file systems options.
        /// </summary>
        public DotNetFileSystemOptions Options { get; }

        /// <inheritdoc />
        public IPropertyStore PropertyStore { get; }

        /// <inheritdoc />
        public ILockManager LockManager { get; }

        /// <inheritdoc />
        public bool SupportsRangedRead { get; } = true;

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
