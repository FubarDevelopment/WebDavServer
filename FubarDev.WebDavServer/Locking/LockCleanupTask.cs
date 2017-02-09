// <copyright file="LockCleanupTask.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FubarDev.WebDavServer.Locking
{
    public class LockCleanupTask : IDisposable
    {
        private static readonly TimeSpan _deactivated = TimeSpan.FromMilliseconds(-1);
        private readonly MultiValueDictionary<DateTime, ActiveLockItem> _activeLocks = new MultiValueDictionary<DateTime, ActiveLockItem>();
        private readonly object _syncRoot = new object();
        private readonly Timer _timer;
        private ActiveLockItem _mostRecentExpirationLockItem;

        public LockCleanupTask()
        {
            _timer = new Timer(TimerExpirationCallback, null, _deactivated, _deactivated);
        }

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

        public void Dispose()
        {
            _timer.Dispose();
        }

        private async void TimerExpirationCallback(object state)
        {
            bool removeResult;
            ActiveLockItem lockItem;
            lock (_syncRoot)
            {
                lockItem = _mostRecentExpirationLockItem;
                removeResult = _activeLocks.Remove(lockItem.Expiration, lockItem);
                var nextLockItem = FindMostRecentExpirationItem();
                if (nextLockItem != null)
                {
                    _mostRecentExpirationLockItem = nextLockItem;
                    ConfigureTimer(nextLockItem);
                }
            }

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
