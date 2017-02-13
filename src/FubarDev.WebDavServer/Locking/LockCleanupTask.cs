// <copyright file="LockCleanupTask.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// A background task that removes expired locks
    /// </summary>
    public class LockCleanupTask : IDisposable
    {
        private static readonly TimeSpan _deactivated = TimeSpan.FromMilliseconds(-1);
        private readonly ISystemClock _systemClock;
        private readonly MultiValueDictionary<DateTime, ActiveLockItem> _activeLocks = new MultiValueDictionary<DateTime, ActiveLockItem>();
        private readonly object _syncRoot = new object();
        private readonly Timer _timer;
        private readonly ILogger<LockCleanupTask> _logger;
        private ActiveLockItem _mostRecentExpirationLockItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="LockCleanupTask"/> class.
        /// </summary>
        /// <param name="systemClock">The system clock</param>
        /// <param name="logger">The logger for the cleanup task</param>
        public LockCleanupTask(
            ISystemClock systemClock,
            ILogger<LockCleanupTask> logger)
        {
            _systemClock = systemClock;
            _logger = logger;
            _timer = new Timer(TimerExpirationCallback, null, _deactivated, _deactivated);
        }

        /// <summary>
        /// Adds a lock to be tracked by this cleanup task.
        /// </summary>
        /// <param name="lockManager">The lock manager that created this active lock.</param>
        /// <param name="activeLock">The active lock to track</param>
        public void Add(ILockManager lockManager, IActiveLock activeLock)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"Adding lock {activeLock}");

            lock (_syncRoot)
            {
                var newLockItem = new ActiveLockItem(lockManager, activeLock);
                _activeLocks.Add(activeLock.Expiration, newLockItem);
                if (_mostRecentExpirationLockItem != null
                    && newLockItem.Expiration >= _mostRecentExpirationLockItem.Expiration)
                {
                    // New item is not the most recent to expire
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug($"New lock {activeLock.StateToken} item is not the most recent item");
                    return;
                }

                _mostRecentExpirationLockItem = newLockItem;
                ConfigureTimer(newLockItem);
            }
        }

        /// <summary>
        /// Removes the active lock so that it isn't tracked any more by this cleanup task.
        /// </summary>
        /// <param name="activeLock">The active lock to remove</param>
        public void Remove(IActiveLock activeLock)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"Try removing lock {activeLock}");

            lock (_syncRoot)
            {
                IReadOnlyCollection<ActiveLockItem> lockItems;
                if (!_activeLocks.TryGetValue(activeLock.Expiration, out lockItems))
                {
                    // Lock item not found
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug($"Lock {activeLock.StateToken} is not tracked any more.");
                    return;
                }

                var lockItem = lockItems
                    .FirstOrDefault(x => string.Equals(x.ActiveLock.StateToken, activeLock.StateToken, StringComparison.Ordinal));

                if (lockItem == null)
                {
                    // Lock item not found
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug($"Lock {activeLock.StateToken} is not tracked any more.");
                    return;
                }

                // Remove lock item
                _activeLocks.Remove(lockItem.Expiration, lockItem);

                if (lockItem.ActiveLock.StateToken == _mostRecentExpirationLockItem.ActiveLock.StateToken)
                {
                    // Removed lock item was the most recent
                    _mostRecentExpirationLockItem = FindMostRecentExpirationItem();
                    if (_mostRecentExpirationLockItem != null)
                    {
                        // Found a new one and reconfigure timer
                        ConfigureTimer(_mostRecentExpirationLockItem);
                    }
                    else if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("No more locks to cleanup.");
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _timer.Dispose();
        }

        /// <summary>
        /// The timer callback which removes an expired item
        /// </summary>
        /// <param name="state">The (unused) state</param>
        private async void TimerExpirationCallback(object state)
        {
            bool removeResult;
            ActiveLockItem lockItem;
            lock (_syncRoot)
            {
                lockItem = _mostRecentExpirationLockItem;
                var now = _systemClock.UtcNow;

                if (_logger.IsEnabled(LogLevel.Trace))
                    _logger.LogTrace($"Cleanup timer called at {now:O}");

                ActiveLockItem nextLockItem;

                // The lock item might be null because a different task might've removed it already
                if (lockItem == null)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug("Lock was already removed (no lock found).");
                    removeResult = false;
                    nextLockItem = FindMostRecentExpirationItem();
                }
                else if (lockItem.Expiration > now)
                {
                    // The expiration might be in the future, because this timer event might be one
                    // that belongs to an already removed item.
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug($"Lock was already removed (different lock {lockItem.ActiveLock.StateToken} found).");
                    removeResult = false;
                    nextLockItem = _mostRecentExpirationLockItem;
                }
                else
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug($"Lock {lockItem.ActiveLock.StateToken} will be removed.");
                    removeResult = _activeLocks.Remove(lockItem.Expiration, lockItem);
                    nextLockItem = FindMostRecentExpirationItem();
                }

                _mostRecentExpirationLockItem = nextLockItem;
                if (_mostRecentExpirationLockItem != null)
                {
                    // There is another lock that needs to be tracked.
                    ConfigureTimer(_mostRecentExpirationLockItem);
                }
                else if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("No more locks to cleanup.");
                }
            }

            // The remove might've failed because different task could've removed
            // it before the timer event could get its hands on it.
            if (removeResult)
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                    _logger.LogTrace($"Lock {lockItem.ActiveLock.StateToken} will now be removed from the lock manager.");
                var stateToken = new Uri(lockItem.ActiveLock.StateToken, UriKind.RelativeOrAbsolute);
                await lockItem.LockManager.ReleaseAsync(stateToken, CancellationToken.None).ConfigureAwait(false);
            }
        }

        private ActiveLockItem FindMostRecentExpirationItem()
        {
            var mostRecentExpiration = _activeLocks.Keys.Select(x => (DateTime?)x).FirstOrDefault();
            if (mostRecentExpiration != null)
            {
                var lockItems = _activeLocks[mostRecentExpiration.Value];
                var nextLockItem = lockItems.First();
                return nextLockItem;
            }

            return null;
        }

        private void ConfigureTimer(ActiveLockItem lockItem)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"Lock {lockItem.ActiveLock.StateToken} is the next to expire.");

            var remainingTime = lockItem.Expiration - _systemClock.UtcNow;
            if (remainingTime < TimeSpan.Zero)
                remainingTime = TimeSpan.Zero;

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"Locks {lockItem.ActiveLock.StateToken} remaining time is {remainingTime}.");

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"Lock {lockItem.ActiveLock.StateToken} is expected to expire at time {_systemClock.UtcNow + remainingTime:O}.");

            _timer.Change(remainingTime, _deactivated);
        }

        private class ActiveLockItem
        {
            public ActiveLockItem(ILockManager lockManager, IActiveLock activeLock)
            {
                LockManager = lockManager;
                ActiveLock = activeLock;
            }

            public DateTime Expiration => ActiveLock.Expiration;

            public ILockManager LockManager { get; }

            public IActiveLock ActiveLock { get; }
        }
    }
}
