// <copyright file="LockCleanupTask.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// A background task that removes expired locks
    /// </summary>
    public class LockCleanupTask : IDisposable
    {
        private static readonly TimeSpan _deactivated = TimeSpan.FromMilliseconds(-1);
        private readonly MultiValueDictionary<DateTime, ActiveLockItem> _activeLocks = new MultiValueDictionary<DateTime, ActiveLockItem>();
        private readonly object _syncRoot = new object();
        private readonly Timer _timer;
        private ActiveLockItem _mostRecentExpirationLockItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="LockCleanupTask"/> class.
        /// </summary>
        public LockCleanupTask()
        {
            _timer = new Timer(TimerExpirationCallback, null, _deactivated, _deactivated);
        }

        /// <summary>
        /// Adds a lock to be tracked by this cleanup task.
        /// </summary>
        /// <param name="lockManager">The lock manager that created this active lock.</param>
        /// <param name="activeLock">The active lock to track</param>
        public void Add(ILockManager lockManager, IActiveLock activeLock)
        {
            lock (_syncRoot)
            {
                var newLockItem = new ActiveLockItem(lockManager, activeLock);
                _activeLocks.Add(activeLock.Expiration, newLockItem);
                if (_mostRecentExpirationLockItem != null
                    && newLockItem.Expiration >= _mostRecentExpirationLockItem.Expiration)
                {
                    // New item is not the most recent to expire
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
            lock (_syncRoot)
            {
                IReadOnlyCollection<ActiveLockItem> lockItems;
                if (!_activeLocks.TryGetValue(activeLock.Expiration, out lockItems))
                {
                    // Lock item not found
                    return;
                }

                var lockItem = lockItems
                    .FirstOrDefault(x => string.Equals(x.ActiveLock.StateToken, activeLock.StateToken, StringComparison.Ordinal));

                if (lockItem == null)
                {
                    // Lock item not found
                    return;
                }

                // Remove lock item
                _activeLocks.Remove(lockItem.Expiration, lockItem);

                if (lockItem.ActiveLock.StateToken == _mostRecentExpirationLockItem.ActiveLock.StateToken)
                {
                    // Removed lock item was the most recent
                    var nextLockItem = FindMostRecentExpirationItem();
                    if (nextLockItem != null)
                    {
                        // Found a new one and reconfigure timer
                        _mostRecentExpirationLockItem = nextLockItem;
                        ConfigureTimer(nextLockItem);
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

                ActiveLockItem nextLockItem;

                // The lock item might be null because a different task might've removed it already
                if (lockItem == null)
                {
                    removeResult = false;
                    nextLockItem = FindMostRecentExpirationItem();
                }
                else if (lockItem.Expiration > DateTime.UtcNow)
                {
                    // The expiration might be in the future, because this timer event might be one
                    // that belongs to an already removed item.
                    removeResult = false;
                    nextLockItem = _mostRecentExpirationLockItem;
                }
                else
                {
                    removeResult = _activeLocks.Remove(lockItem.Expiration, lockItem);
                    nextLockItem = FindMostRecentExpirationItem();
                }

                if (nextLockItem != null)
                {
                    // There is another lock that needs to be tracked.
                    _mostRecentExpirationLockItem = nextLockItem;
                    ConfigureTimer(nextLockItem);
                }
            }

            // The remove might've failed because different task could've removed
            // it before the timer event could get its hands on it.
            if (removeResult)
            {
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
            var remainingTime = lockItem.Expiration - DateTime.UtcNow;
            if (remainingTime < TimeSpan.Zero)
                remainingTime = TimeSpan.Zero;

            // Round up to the next full second to avoid problems with
            // timer inaccuracies
            var addSec = remainingTime.Milliseconds != 0 ? 1 : 0;
            remainingTime = new TimeSpan(remainingTime.Days, remainingTime.Hours, remainingTime.Minutes, remainingTime.Seconds)
                .Add(TimeSpan.FromSeconds(addSec));

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
