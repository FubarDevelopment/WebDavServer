// <copyright file="InMemoryLockManager.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Utils;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Locking.InMemory
{
    /// <summary>
    /// An in-memory implementation of a lock manager.
    /// </summary>
    public class InMemoryLockManager : LockManagerBase
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private IImmutableDictionary<string, IActiveLock> _locks = ImmutableDictionary<string, IActiveLock>.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryLockManager"/> class.
        /// </summary>
        /// <param name="options">The options of the lock manager.</param>
        /// <param name="litmusCompatibilityOptions">The compatibility options for the litmus tests.</param>
        /// <param name="cleanupTask">The clean-up task for expired locks.</param>
        /// <param name="systemClock">The system clock interface.</param>
        /// <param name="logger">The logger.</param>
        public InMemoryLockManager(
            IOptions<InMemoryLockManagerOptions> options,
            IOptions<LitmusCompatibilityOptions> litmusCompatibilityOptions,
            ILockCleanupTask cleanupTask,
            ISystemClock systemClock,
            ILogger<InMemoryLockManager> logger)
            : base(litmusCompatibilityOptions, cleanupTask, systemClock, logger, options.Value)
        {
        }

        /// <inheritdoc />
        protected override async Task<ILockManagerTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            return new InMemoryTransaction(this);
        }

        private class InMemoryTransaction : ILockManagerTransaction
        {
            private readonly InMemoryLockManager _lockManager;

            private IImmutableDictionary<string, IActiveLock> _locks;

            /// <summary>
            /// Initializes a new instance of the <see cref="InMemoryTransaction"/> class.
            /// </summary>
            /// <param name="lockManager">The lock manager that stores the locks.</param>
            public InMemoryTransaction(InMemoryLockManager lockManager)
            {
                _lockManager = lockManager;
                _locks = lockManager._locks;
            }

            /// <inheritdoc />
            public Task<IReadOnlyCollection<IActiveLock>> GetActiveLocksAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult<IReadOnlyCollection<IActiveLock>>(_locks.Values.ToList());
            }

            /// <inheritdoc />
            public Task<bool> AddAsync(IActiveLock activeLock, CancellationToken cancellationToken)
            {
                if (_locks.ContainsKey(activeLock.StateToken))
                {
                    return Task.FromResult(false);
                }

                _locks = _locks.Add(activeLock.StateToken, activeLock);
                return Task.FromResult(true);
            }

            /// <inheritdoc />
            public Task<bool> UpdateAsync(IActiveLock activeLock, CancellationToken cancellationToken)
            {
                var hadKey = _locks.ContainsKey(activeLock.StateToken);
                if (hadKey)
                {
                    _locks = _locks.Remove(activeLock.StateToken);
                }

                _locks = _locks.Add(activeLock.StateToken, activeLock);
                return Task.FromResult(hadKey);
            }

            /// <inheritdoc />
            public Task<bool> RemoveAsync(string stateToken, CancellationToken cancellationToken)
            {
                if (!_locks.ContainsKey(stateToken))
                {
                    return Task.FromResult(false);
                }

                _locks = _locks.Remove(stateToken);
                return Task.FromResult(true);
            }

            /// <inheritdoc />
            public Task<IActiveLock?> GetAsync(string stateToken, CancellationToken cancellationToken)
            {
                _locks.TryGetValue(stateToken, out var activeLock);

                // ReSharper disable once RedundantTypeArgumentsOfMethod
                return Task.FromResult<IActiveLock?>(activeLock);
            }

            /// <inheritdoc />
            public Task CommitAsync(CancellationToken cancellationToken)
            {
                _lockManager._locks = _locks;
                return Task.CompletedTask;
            }

            /// <inheritdoc />
            public void Dispose()
            {
                _lockManager._semaphore.Release();
            }
        }
    }
}
