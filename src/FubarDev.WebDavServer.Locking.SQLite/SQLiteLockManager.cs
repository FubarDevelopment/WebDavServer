// <copyright file="SQLiteLockManager.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using sqlitenet = SQLite;

namespace FubarDev.WebDavServer.Locking.SQLite
{
    /// <summary>
    /// An implementation of <see cref="ILockManager"/> that uses SQLite
    /// </summary>
    public class SQLiteLockManager : LockManagerBase
    {
        [NotNull]
        private readonly sqlitenet.SQLiteConnection _connection;

        private readonly object _initSync = new object();

        private volatile bool _initialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteLockManager"/> class.
        /// </summary>
        /// <param name="sqliteOptions">The SQLite options</param>
        /// <param name="cleanupTask">The clean-up task for expired locks</param>
        /// <param name="systemClock">The system clock interface</param>
        /// <param name="logger">The logger</param>
        public SQLiteLockManager(
            [NotNull] IOptions<SQLiteLockManagerOptions> sqliteOptions,
            [NotNull] ILockCleanupTask cleanupTask,
            [NotNull] ISystemClock systemClock,
            [NotNull] ILogger<SQLiteLockManager> logger)
            : base(cleanupTask, systemClock, logger, sqliteOptions.Value)
        {
            if (string.IsNullOrEmpty(sqliteOptions.Value.DatabaseFileName))
                throw new ArgumentException("A database file name must be set in the SQLiteLockManager options.");
            EnsureDatabaseExists(sqliteOptions.Value.DatabaseFileName);
            _connection = new sqlitenet.SQLiteConnection(sqliteOptions.Value.DatabaseFileName);
        }

        /// <summary>
        /// Ensures that a database with the given file name exists.
        /// </summary>
        /// <param name="dbFileName">The file name of the database</param>
        public static void EnsureDatabaseExists(string dbFileName)
        {
            if (File.Exists(dbFileName))
            {
                using (var conn = new sqlitenet.SQLiteConnection(dbFileName))
                {
                    CreateDatabaseTables(conn);
                }

                return;
            }

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
                CreateDatabaseTables(conn);
            }
        }

        /// <inheritdoc />
        protected override Task<ILockManagerTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            if (!_initialized)
            {
                lock (_initSync)
                {
                    if (!_initialized)
                    {
                        // Load all active locks and add them to the cleanup task.
                        // This ensures that locks still do expire.
                        var activeLocks = _connection.Table<ActiveLockEntry>().ToList();
                        foreach (var activeLock in activeLocks)
                        {
                            LockCleanupTask.Add(this, activeLock);
                        }

                        _initialized = true;
                    }
                }
            }

            _connection.BeginTransaction();
            return Task.FromResult<ILockManagerTransaction>(new SQLiteLockManagerTransaction(_connection));
        }

        /// <summary>
        /// Creates the database tables
        /// </summary>
        /// <param name="connection">The database connection</param>
        private static void CreateDatabaseTables(sqlitenet.SQLiteConnection connection)
        {
            connection.CreateTable<ActiveLockEntry>(sqlitenet.CreateFlags.AllImplicit);
        }

        private class SQLiteLockManagerTransaction : ILockManagerTransaction
        {
            [NotNull]
            private readonly sqlitenet.SQLiteConnection _connection;

            private bool _committed;

            public SQLiteLockManagerTransaction([NotNull] sqlitenet.SQLiteConnection connection)
            {
                _connection = connection;
            }

            public Task<IReadOnlyCollection<IActiveLock>> GetActiveLocksAsync(CancellationToken cancellationToken)
            {
                var locks = _connection.Table<ActiveLockEntry>().Cast<IActiveLock>().ToList();
                return Task.FromResult<IReadOnlyCollection<IActiveLock>>(locks);
            }

            public Task<bool> AddAsync(IActiveLock activeLock, CancellationToken cancellationToken)
            {
                var entry = ToEntry(activeLock);
                var affectedRows = _connection.Insert(entry);
                return Task.FromResult(affectedRows != 0);
            }

            public Task<bool> UpdateAsync(IActiveLock activeLock, CancellationToken cancellationToken)
            {
                var entry = ToEntry(activeLock);
                _connection.InsertOrReplace(entry);
                return Task.FromResult(true);
            }

            public Task<bool> RemoveAsync(string stateToken, CancellationToken cancellationToken)
            {
                var affectedRows = _connection.Table<ActiveLockEntry>().Delete(x => x.StateToken == stateToken);
                return Task.FromResult(affectedRows != 0);
            }

            public Task<IActiveLock> GetAsync(string stateToken, CancellationToken cancellationToken)
            {
                var l = _connection.Table<ActiveLockEntry>().FirstOrDefault(x => x.StateToken == stateToken);
                return Task.FromResult<IActiveLock>(l);
            }

            public Task CommitAsync(CancellationToken cancellationToken)
            {
                _connection.Commit();
                _committed = true;
                return Task.FromResult(0);
            }

            public void Dispose()
            {
                if (!_committed)
                    _connection.Rollback();
            }

            private ActiveLockEntry ToEntry(IActiveLock activeLock)
            {
                return activeLock as ActiveLockEntry
                       ?? new ActiveLockEntry()
                       {
                           StateToken = activeLock.StateToken,
                           Path = activeLock.Path,
                           Recursive = activeLock.Recursive,
                           Href = activeLock.Href,
                           Owner = activeLock.GetOwner()?.ToString(SaveOptions.OmitDuplicateNamespaces),
                           AccessType = activeLock.AccessType,
                           ShareMode = activeLock.ShareMode,
                           Timeout = activeLock.Timeout,
                           Expiration = activeLock.Expiration,
                           Issued = activeLock.Issued,
                           LastRefresh = activeLock.LastRefresh,
                       };
            }
        }
    }
}
