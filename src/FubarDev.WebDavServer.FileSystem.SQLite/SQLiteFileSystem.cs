// <copyright file="SQLiteFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem.Mount;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Props.Store;

using db = SQLite;

namespace FubarDev.WebDavServer.FileSystem.SQLite
{
    /// <summary>
    /// The <see cref="ILocalFileSystem"/> implementation using an SQLite database
    /// </summary>
    public class SQLiteFileSystem : ILocalFileSystem, IDisposable, IMountPointManager
    {
        private readonly db::SQLiteConnection _connection;

        private readonly IPathTraversalEngine _pathTraversalEngine;

        private readonly Dictionary<Uri, IFileSystem> _mountPoints = new Dictionary<Uri, IFileSystem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteFileSystem"/> class.
        /// </summary>
        /// <param name="options">The options for this file system.</param>
        /// <param name="mountPoint">The mount point where this file system should be included.</param>
        /// <param name="connection">The SQLite database connection.</param>
        /// <param name="pathTraversalEngine">The engine to traverse paths.</param>
        /// <param name="lockManager">The global lock manager.</param>
        /// <param name="propertyStoreFactory">The store for dead properties.</param>
        public SQLiteFileSystem(
            SQLiteFileSystemOptions options,
            ICollection? mountPoint,
            db::SQLiteConnection connection,
            IPathTraversalEngine pathTraversalEngine,
            ILockManager? lockManager = null,
            IPropertyStoreFactory? propertyStoreFactory = null)
        {
            RootDirectoryPath = Path.GetDirectoryName(connection.DatabasePath)!;
            LockManager = lockManager;
            _connection = connection;
            _pathTraversalEngine = pathTraversalEngine;
            Options = options;
            PropertyStore = propertyStoreFactory?.Create(this);
            var rootEntry = connection.Table<FileEntry>().Where(x => x.Id == string.Empty).ToList().Single();
            var rootPath = mountPoint?.Path ?? new Uri(string.Empty, UriKind.Relative);
            var rootDir = new SQLiteCollection(this, mountPoint, rootEntry, rootPath, mountPoint?.Name ?? rootPath.GetName(), true);
            Root = new AsyncLazy<ICollection>(() => Task.FromResult<ICollection>(rootDir));
        }

        /// <summary>
        /// Gets the file systems options.
        /// </summary>
        public SQLiteFileSystemOptions Options { get; }

        /// <summary>
        /// Gets the SQLite DB connection.
        /// </summary>
        public db::SQLiteConnection Connection => _connection;

        /// <inheritdoc />
        public AsyncLazy<ICollection> Root { get; }

        /// <inheritdoc />
        public bool SupportsRangedRead { get; } = true;

        /// <inheritdoc />
        public IPropertyStore? PropertyStore { get; }

        /// <inheritdoc />
        public ILockManager? LockManager { get; }

        /// <inheritdoc />
        public string RootDirectoryPath { get; }

        /// <inheritdoc />
        public bool HasSubfolders { get; } = false;

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

        /// <inheritdoc />
        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
