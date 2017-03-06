// <copyright file="SQLiteFileSystemFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Security.Principal;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Store;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

using sqlite3 = SQLite.SQLite3;
using sqlitenet = SQLite;

namespace FubarDev.WebDavServer.FileSystem.SQLite
{
    /// <summary>
    /// An implementation of <see cref="IFileSystemFactory"/> that provides file system storage in a SQLite database
    /// </summary>
    public class SQLiteFileSystemFactory : IFileSystemFactory
    {
        [NotNull]
        private readonly PathTraversalEngine _pathTraversalEngine;

        [NotNull]
        private readonly IDeadPropertyFactory _deadPropertyFactory;

        [CanBeNull]
        private readonly IPropertyStoreFactory _propertyStoreFactory;

        [CanBeNull]
        private readonly ILockManager _lockManager;

        [NotNull]
        private readonly SQLiteFileSystemOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteFileSystemFactory"/> class.
        /// </summary>
        /// <param name="options">The options for this file system</param>
        /// <param name="pathTraversalEngine">The engine to traverse paths</param>
        /// <param name="deadPropertyFactory">A factory for dead properties</param>
        /// <param name="propertyStoreFactory">The store for dead properties</param>
        /// <param name="lockManager">The global lock manager</param>
        public SQLiteFileSystemFactory(
            [NotNull] IOptions<SQLiteFileSystemOptions> options,
            [NotNull] PathTraversalEngine pathTraversalEngine,
            [NotNull] IDeadPropertyFactory deadPropertyFactory,
            [CanBeNull] IPropertyStoreFactory propertyStoreFactory = null,
            [CanBeNull] ILockManager lockManager = null)
        {
            _pathTraversalEngine = pathTraversalEngine;
            _deadPropertyFactory = deadPropertyFactory;
            _propertyStoreFactory = propertyStoreFactory;
            _lockManager = lockManager;
            _options = options.Value;
        }

        /// <summary>
        /// Ensures that a database with the given file name exists.
        /// </summary>
        /// <param name="dbFileName">The file name of the database</param>
        public static void EnsureDatabaseExists(string dbFileName)
        {
            if (File.Exists(dbFileName))
                return;
            CreateDatabase(dbFileName);
        }

        /// <summary>
        /// Creates a new database
        /// </summary>
        /// <param name="dbFileName">The file name of the database</param>
        public static void CreateDatabase(string dbFileName)
        {
            if (File.Exists(dbFileName))
                File.Delete(dbFileName);
            using (var conn = new sqlitenet.SQLiteConnection(dbFileName))
            {
                conn.CreateTable<FileEntry>(sqlitenet.CreateFlags.AllImplicit);
                conn.CreateTable<FileData>(sqlitenet.CreateFlags.AllImplicit);
                conn.Insert(new FileEntry()
                {
                    Id = string.Empty,
                    IsCollection = true,
                    Name = string.Empty,
                    Path = string.Empty,
                });
            }
        }

        /// <inheritdoc />
        public IFileSystem CreateFileSystem(IPrincipal principal)
        {
            var userHomePath = Utils.SystemInfo.GetUserHomePath(
                principal,
                homePath: _options.RootPath,
                anonymousUserName: "anonymous");

            var dbDir = Path.GetDirectoryName(userHomePath);
            var dbName = Path.GetFileName(userHomePath) + ".db";
            var dbFileName = Path.Combine(dbDir, dbName);

            Directory.CreateDirectory(dbDir);
            EnsureDatabaseExists(dbFileName);

            var conn = new sqlitenet.SQLiteConnection(dbFileName);
            return new SQLiteFileSystem(_options, conn, _pathTraversalEngine, _deadPropertyFactory, _lockManager, _propertyStoreFactory);
        }
    }
}
