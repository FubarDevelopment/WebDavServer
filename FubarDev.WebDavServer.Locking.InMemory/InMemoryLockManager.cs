// <copyright file="InMemoryLockManager.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LanguageExt;
using static LanguageExt.Prelude;

namespace FubarDev.WebDavServer.Locking.InMemory
{
    public class InMemoryLockManager : ILockManager
    {
        private static readonly Uri _baseUrl = new Uri("http://localhost/");

        private static readonly IReadOnlyCollection<IActiveLock> _emptyActiveLocks = new ActiveLock[0];

        private readonly object _syncRoot = new object();

        private readonly LockCleanupTask _cleanupTask;

        private readonly ISystemClock _systemClock;

        private IImmutableDictionary<Uri, IActiveLock> _locks = ImmutableDictionary<Uri, IActiveLock>.Empty;

        public InMemoryLockManager(LockCleanupTask cleanupTask, ISystemClock systemClock)
        {
            _cleanupTask = cleanupTask;
            _systemClock = systemClock;
        }

        public event EventHandler<LockEventArgs> LockAdded;

        public event EventHandler<LockEventArgs> LockReleased;

        public Task<Either<IReadOnlyCollection<IActiveLock>, IActiveLock>> LockAsync(ILock l, CancellationToken cancellationToken)
        {
            IActiveLock newActiveLock;
            var destinationUrl = new Uri(_baseUrl, l.Path);
            lock (_syncRoot)
            {
                var status = Find(destinationUrl, l.Recursive);
                var conflictingLocks = GetConflictingLocks(status, LockShareMode.Parse(l.ShareMode));
                if (conflictingLocks.Count != 0)
                    return Task.FromResult(Left<IReadOnlyCollection<IActiveLock>, IActiveLock>(conflictingLocks));

                newActiveLock = new ActiveLock(l, _systemClock.UtcNow);
                var stateToken = new Uri(newActiveLock.StateToken);
                _locks = _locks.Add(stateToken, newActiveLock);
            }

            OnLockAdded(newActiveLock);

            _cleanupTask.Add(this, newActiveLock);

            return Task.FromResult(Right<IReadOnlyCollection<IActiveLock>, IActiveLock>(newActiveLock));
        }

        public Task<bool> ReleaseAsync(Uri stateToken, CancellationToken cancellationToken)
        {
            IActiveLock activeLock;
            lock (_syncRoot)
            {
                if (!_locks.TryGetValue(stateToken, out activeLock))
                    return Task.FromResult(false);

                _locks = _locks.Remove(stateToken);
            }

            _cleanupTask.Remove(activeLock);

            OnLockReleased(activeLock);

            return Task.FromResult(true);
        }

        public Task<IEnumerable<IActiveLock>> GetLocksAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_locks.Values);
        }

        protected virtual void OnLockAdded(IActiveLock activeLock)
        {
            LockAdded?.Invoke(this, new LockEventArgs(activeLock));
        }

        protected virtual void OnLockReleased(IActiveLock activeLock)
        {
            LockReleased?.Invoke(this, new LockEventArgs(activeLock));
        }

        private static IReadOnlyCollection<IActiveLock> GetConflictingLocks(LockStatus affactingLocks, LockShareMode shareMode)
        {
            if (shareMode.Id == LockShareMode.Exclusive.Id)
            {
                if (affactingLocks.IsEmpty)
                    return _emptyActiveLocks;
                return affactingLocks.GetLocks().ToList();
            }

            var exclusiveLocks =
                (from activeLock in affactingLocks.GetLocks()
                 let lockShareMode = LockShareMode.Parse(activeLock.ShareMode)
                 where lockShareMode.Id == LockShareMode.Exclusive.Id
                 select activeLock)
                .ToList();
            return exclusiveLocks;
        }

        private LockStatus Find(Uri destinationUrl, bool withChildren)
        {
            var refLocks = new List<IActiveLock>();
            var childLocks = new List<IActiveLock>();
            var parentLocks = new List<IActiveLock>();

            foreach (var activeLock in _locks.Values)
            {
                var lockUrl = new Uri(_baseUrl, activeLock.Path);
                if (destinationUrl == lockUrl)
                {
                    refLocks.Add(activeLock);
                }
                else if (withChildren && destinationUrl.IsBaseOf(lockUrl))
                {
                    childLocks.Add(activeLock);
                }
                else if (activeLock.Recursive && lockUrl.IsBaseOf(destinationUrl))
                {
                    parentLocks.Add(activeLock);
                }
            }

            return new LockStatus(refLocks, parentLocks, childLocks);
        }
    }
}
