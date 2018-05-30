// <copyright file="NHibernateFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.Mount;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.NHibernate.Models;
using FubarDev.WebDavServer.Props.Store;

using JetBrains.Annotations;

using NHibernate;

namespace FubarDev.WebDavServer.NHibernate.FileSystem
{
    /// <summary>
    /// The <see cref="IFileSystem"/> implementation using an SQLite database
    /// </summary>
    public class NHibernateFileSystem : IFileSystem, IMountPointManager
    {
        [NotNull]
        private readonly IPathTraversalEngine _pathTraversalEngine;

        private readonly Dictionary<Uri, IFileSystem> _mountPoints = new Dictionary<Uri, IFileSystem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateFileSystem"/> class.
        /// </summary>
        /// <param name="mountPoint">The mount point where this file system should be included</param>
        /// <param name="connection">The SQLite database connection</param>
        /// <param name="pathTraversalEngine">The engine to traverse paths</param>
        /// <param name="lockManager">The global lock manager</param>
        /// <param name="propertyStoreFactory">The store for dead properties</param>
        public NHibernateFileSystem(
            [CanBeNull] ICollection mountPoint,
            [NotNull] ISession connection,
            [NotNull] IPathTraversalEngine pathTraversalEngine,
            [CanBeNull] ILockManager lockManager = null,
            [CanBeNull] IPropertyStoreFactory propertyStoreFactory = null)
        {
            LockManager = lockManager;
            Connection = connection;
            _pathTraversalEngine = pathTraversalEngine;
            PropertyStore = propertyStoreFactory?.Create(this);
            Root = new AsyncLazy<ICollection>(async () =>
            {
                var rootEntry = await connection.LoadAsync<FileEntry>(Guid.Empty);
                var rootPath = mountPoint?.Path ?? new Uri(string.Empty, UriKind.Relative);
                var rootDir = new NHibernateCollection(this, mountPoint, rootEntry, rootPath, mountPoint?.Name ?? rootPath.GetName(), true);
                return rootDir;
            });
        }

        /// <summary>
        /// Gets the SQLite DB connection
        /// </summary>
        [NotNull]
        public ISession Connection { get; }

        /// <inheritdoc />
        public AsyncLazy<ICollection> Root { get; }

        /// <inheritdoc />
        public bool SupportsRangedRead { get; } = true;

        /// <inheritdoc />
        public IPropertyStore PropertyStore { get; }

        /// <inheritdoc />
        public ILockManager LockManager { get; }

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
