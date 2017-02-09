// <copyright file="InMemoryLockManager.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using LanguageExt;
using static LanguageExt.Prelude;

namespace FubarDev.WebDavServer.Locking.InMemory
{
    public class InMemoryLockManager : ILockManager
    {
        private static readonly Uri _baseUrl = new Uri("http://localhost/");

        private readonly LockCleanupTask _cleanupTask;

        private readonly object _syncRoot = new object();

        private IImmutableDictionary<string, IActiveLock> _locks = ImmutableDictionary<string, IActiveLock>.Empty;

        public InMemoryLockManager()
        {
            _cleanupTask = new LockCleanupTask();
        }

        public Task<Either<IReadOnlyCollection<IActiveLock>, IActiveLock>> LockAsync(ILock l, CancellationToken cancellationToken)
        {
            var destinationUrl = new Uri(_baseUrl, l.Path);
            lock (_syncRoot)
            {
                var status = Find(destinationUrl);
                var conflictingLocks = GetConflictingLocks(l.ShareMode, status);
                if (conflictingLocks.Count != 0)
                    return Task.FromResult(Left<IReadOnlyCollection<IActiveLock>, IActiveLock>(conflictingLocks));

                var newActiveLock = new ActiveLock(l);
                _locks = _locks.Add(newActiveLock.StateToken, newActiveLock);
                _cleanupTask.Add(this, newActiveLock);
                return Task.FromResult(Right<IReadOnlyCollection<IActiveLock>, IActiveLock>(newActiveLock));
            }
        }

        public Task ReleaseAsync(Uri stateToken, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IActiveLock>> GetLocksAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_locks.Values);
        }

        private IReadOnlyCollection<IActiveLock> GetConflictingLocks(string shareMode, LockStatus affactingLocks)
        {
            throw new NotImplementedException();
        }

        private LockStatus Find(Uri destinationUrl)
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
                else if (destinationUrl.IsBaseOf(lockUrl))
                {
                    childLocks.Add(activeLock);
                }
                else if (lockUrl.IsBaseOf(destinationUrl))
                {
                    parentLocks.Add(activeLock);
                }
            }

            return new LockStatus(refLocks, parentLocks, childLocks);
        }
    }
}
