// <copyright file="NHibernateLockManager.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.NHibernate.Models;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NHibernate;
using NHibernate.Linq;

namespace FubarDev.WebDavServer.NHibernate.Locking
{
    /// <summary>
    /// An implementation of <see cref="ILockManager"/> that uses SQLite
    /// </summary>
    public class NHibernateLockManager : LockManagerBase, IDisposable
    {
        [NotNull]
        private readonly ISession _connection;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private readonly object _initSync = new object();

        private volatile bool _initialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateLockManager"/> class.
        /// </summary>
        /// <param name="sessionFactory">The NHibernate session factory</param>
        /// <param name="options">The options</param>
        /// <param name="cleanupTask">The clean-up task for expired locks</param>
        /// <param name="systemClock">The system clock interface</param>
        /// <param name="logger">The logger</param>
        public NHibernateLockManager(
            [NotNull] ISessionFactory sessionFactory,
            [NotNull] IOptions<NHibernateLockManagerOptions> options,
            [NotNull] ILockCleanupTask cleanupTask,
            [NotNull] ISystemClock systemClock,
            [NotNull] ILogger<NHibernateLockManager> logger)
            : this(sessionFactory.OpenSession(), options.Value, cleanupTask, systemClock, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateLockManager"/> class.
        /// </summary>
        /// <param name="session">The NHibernate session</param>
        /// <param name="options">The options</param>
        /// <param name="cleanupTask">The clean-up task for expired locks</param>
        /// <param name="systemClock">The system clock interface</param>
        /// <param name="logger">The logger</param>
        public NHibernateLockManager(
            [NotNull] ISession session,
            [NotNull] NHibernateLockManagerOptions options,
            [NotNull] ILockCleanupTask cleanupTask,
            [NotNull] ISystemClock systemClock,
            [NotNull] ILogger<NHibernateLockManager> logger)
            : base(cleanupTask, systemClock, logger, options)
        {
            _connection = session;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _connection.Dispose();
        }

        /// <inheritdoc />
        protected override async Task<ILockManagerTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            if (!_initialized)
            {
                lock (_initSync)
                {
                    if (!_initialized)
                    {
                        // Load all active locks and add them to the cleanup task.
                        // This ensures that locks still do expire.
                        var activeLocks = _connection.Query<ActiveLockEntry>().ToList();
                        foreach (var activeLock in activeLocks)
                        {
                            LockCleanupTask.Add(this, activeLock);
                        }

                        _initialized = true;
                    }
                }
            }

            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            var trans = _connection.BeginTransaction();
            return new SQLiteLockManagerTransaction(_connection, trans, _semaphore);
        }

        private class SQLiteLockManagerTransaction : ILockManagerTransaction
        {
            [NotNull]
            private readonly ISession _connection;

            [NotNull]
            private readonly ITransaction _transaction;

            [NotNull]
            private readonly SemaphoreSlim _semaphore;

            private bool _committed;

            public SQLiteLockManagerTransaction([NotNull] ISession connection, [NotNull] ITransaction transaction, [NotNull] SemaphoreSlim semaphore)
            {
                _connection = connection;
                _transaction = transaction;
                _semaphore = semaphore;
            }

            public async Task<IReadOnlyCollection<IActiveLock>> GetActiveLocksAsync(CancellationToken cancellationToken)
            {
                return await _connection.Query<ActiveLockEntry>().ToListAsync(cancellationToken);
            }

            public async Task<bool> AddAsync(IActiveLock activeLock, CancellationToken cancellationToken)
            {
                var entry = ToEntry(activeLock);
                await _connection.SaveAsync(entry, cancellationToken)
                    .ConfigureAwait(false);
                return true;
            }

            public async Task<bool> UpdateAsync(IActiveLock activeLock, CancellationToken cancellationToken)
            {
                var entry = ToEntry(activeLock);
                await _connection.SaveOrUpdateAsync(entry, cancellationToken);
                return true;
            }

            public async Task<bool> RemoveAsync(string stateToken, CancellationToken cancellationToken)
            {
                var affectedRows = await _connection.CreateQuery("delete ActiveLockEntry l where l.StateToken=?")
                    .SetParameter(0, stateToken)
                    .ExecuteUpdateAsync(cancellationToken)
                    .ConfigureAwait(false);
                return affectedRows != 0;
            }

            public async Task<IActiveLock> GetAsync(string stateToken, CancellationToken cancellationToken)
            {
                return await _connection.GetAsync<ActiveLockEntry>(stateToken, cancellationToken)
                    .ConfigureAwait(false);
            }

            public Task CommitAsync(CancellationToken cancellationToken)
            {
                _transaction.Commit();
                _committed = true;
                return Task.CompletedTask;
            }

            public void Dispose()
            {
                if (!_committed)
                    _transaction.Rollback();
                _transaction.Dispose();
                _connection.Clear();
                _semaphore.Release();
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
