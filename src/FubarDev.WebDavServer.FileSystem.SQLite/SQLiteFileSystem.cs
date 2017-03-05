// <copyright file="SQLiteFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Store;

using JetBrains.Annotations;

using db = SQLite;

namespace FubarDev.WebDavServer.FileSystem.SQLite
{
    public class SQLiteFileSystem : ILocalFileSystem, IDisposable
    {
        [NotNull]
        private readonly db::SQLiteConnection _connection;

        [NotNull]
        private readonly PathTraversalEngine _pathTraversalEngine;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteFileSystem"/> class.
        /// </summary>
        /// <param name="options">The options for this file system</param>
        /// <param name="connection">The SQLite database connection</param>
        /// <param name="pathTraversalEngine">The engine to traverse paths</param>
        /// <param name="deadPropertyFactory">A factory for dead properties</param>
        /// <param name="lockManager">The global lock manager</param>
        /// <param name="propertyStoreFactory">The store for dead properties</param>
        public SQLiteFileSystem(
            [NotNull] SQLiteFileSystemOptions options,
            [NotNull] db::SQLiteConnection connection,
            [NotNull] PathTraversalEngine pathTraversalEngine,
            [NotNull] IDeadPropertyFactory deadPropertyFactory,
            [CanBeNull] ILockManager lockManager = null,
            [CanBeNull] IPropertyStoreFactory propertyStoreFactory = null)
        {
            RootDirectoryPath = Path.GetDirectoryName(connection.DatabasePath);
            LockManager = lockManager;
            DeadPropertyFactory = deadPropertyFactory;
            _connection = connection;
            _pathTraversalEngine = pathTraversalEngine;
            Options = options;
            PropertyStore = propertyStoreFactory?.Create(this);
            var rootEntry = connection.Table<FileEntry>().Where(x => x.Id == string.Empty).ToList().Single();
            var rootDir = new SQLiteCollection(this, null, rootEntry, new Uri(string.Empty, UriKind.Relative));
            Root = new AsyncLazy<ICollection>(() => Task.FromResult<ICollection>(rootDir));
        }

        /// <summary>
        /// Gets the file systems options
        /// </summary>
        public SQLiteFileSystemOptions Options { get; }

        /// <summary>
        /// Gets the factory for dead properties
        /// </summary>
        public IDeadPropertyFactory DeadPropertyFactory { get; }

        /// <summary>
        /// Gets the SQLite DB connection
        /// </summary>
        public db::SQLiteConnection Connection => _connection;

        /// <inheritdoc />
        public AsyncLazy<ICollection> Root { get; }

        /// <inheritdoc />
        public bool SupportsRangedRead { get; } = true;

        /// <inheritdoc />
        public IPropertyStore PropertyStore { get; }

        /// <inheritdoc />
        public ILockManager LockManager { get; }

        /// <inheritdoc />
        public string RootDirectoryPath { get; }

        /// <inheritdoc />
        public bool HasSubfolders { get; } = false;

        /// <inheritdoc />
        public Task<SelectionResult> SelectAsync(string path, CancellationToken ct)
        {
            return _pathTraversalEngine.TraverseAsync(this, path, ct);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
