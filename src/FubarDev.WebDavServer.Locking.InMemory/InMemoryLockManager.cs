// <copyright file="InMemoryLockManager.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Locking.InMemory
{
    public class InMemoryLockManager : ILockManager
    {
        private static readonly Uri _baseUrl = new Uri("http://localhost/");

        private readonly object _syncRoot = new object();

        private readonly LockCleanupTask _cleanupTask;

        private readonly ISystemClock _systemClock;

        private readonly ILogger<InMemoryLockManager> _logger;

        private readonly ILockTimeRounding _rounding;

        private IImmutableDictionary<Uri, IActiveLock> _locks = ImmutableDictionary<Uri, IActiveLock>.Empty;

        public InMemoryLockManager(IOptions<LockManagerOptions> options, LockCleanupTask cleanupTask, ISystemClock systemClock, ILogger<InMemoryLockManager> logger)
        {
            var opt = options?.Value ?? new LockManagerOptions();
            _rounding = opt.Rounding ?? new DefaultLockTimeRounding(DefaultLockTimeRoundingMode.OneSecond);
            _cleanupTask = cleanupTask;
            _systemClock = systemClock;
            _logger = logger;
        }

        public event EventHandler<LockEventArgs> LockAdded;

        public event EventHandler<LockEventArgs> LockReleased;

        private enum LockCompareResult
        {
            RightIsParent,
            LeftIsParent,
            Reference,
            NoMatch,
        }

        public Task<LockResult> LockAsync(ILock l, CancellationToken cancellationToken)
        {
            IActiveLock newActiveLock;
            var destinationUrl = new Uri(_baseUrl, l.Path);
            lock (_syncRoot)
            {
                var status = Find(destinationUrl, l.Recursive);
                var conflictingLocks = GetConflictingLocks(status, LockShareMode.Parse(l.ShareMode));
                if (conflictingLocks.Count != 0)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation($"Found conflicting locks for {l}: {string.Join(",", conflictingLocks.GetLocks().Select(x => x.ToString()))}");
                    return Task.FromResult(new LockResult(conflictingLocks));
                }

                newActiveLock = new ActiveLock(l, _rounding.Round(_systemClock.UtcNow), _rounding.Round(l.Timeout));
                var stateToken = new Uri(newActiveLock.StateToken);
                _locks = _locks.Add(stateToken, newActiveLock);
            }

            OnLockAdded(newActiveLock);

            _cleanupTask.Add(this, newActiveLock);

            return Task.FromResult(new LockResult(newActiveLock));
        }

        public Task<LockReleaseStatus> ReleaseAsync(string path, Uri stateToken, CancellationToken cancellationToken)
        {
            IActiveLock activeLock;
            lock (_syncRoot)
            {
                if (!_locks.TryGetValue(stateToken, out activeLock))
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation($"Tried to remove non-existent lock {stateToken}");
                    return Task.FromResult(LockReleaseStatus.NoLock);
                }

                var destinationUrl = new Uri(_baseUrl, path);
                var lockUrl = new Uri(_baseUrl, activeLock.Path);
                var lockCompareResult = Compare(lockUrl, activeLock.Recursive, destinationUrl, false);
                if (lockCompareResult != LockCompareResult.Reference)
                    return Task.FromResult(LockReleaseStatus.InvalidLockRange);

                _locks = _locks.Remove(stateToken);
            }

            // ReSharper disable once InconsistentlySynchronizedField
            _cleanupTask.Remove(activeLock);

            OnLockReleased(activeLock);

            return Task.FromResult(LockReleaseStatus.Success);
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

        private static LockStatus GetConflictingLocks(LockStatus affactingLocks, LockShareMode shareMode)
        {
            if (shareMode == LockShareMode.Exclusive)
            {
                return affactingLocks;
            }

            return new LockStatus(
                affactingLocks
                    .ReferenceLocks
                    .Where(x => LockShareMode.Parse(x.ShareMode) == LockShareMode.Exclusive)
                    .ToList(),
                affactingLocks
                    .ParentLocks
                    .Where(x => LockShareMode.Parse(x.ShareMode) == LockShareMode.Exclusive)
                    .ToList(),
                affactingLocks
                    .ChildLocks
                    .Where(x => LockShareMode.Parse(x.ShareMode) == LockShareMode.Exclusive)
                    .ToList());
        }

        private LockStatus Find(Uri destinationUrl, bool withChildren)
        {
            var refLocks = new List<IActiveLock>();
            var childLocks = new List<IActiveLock>();
            var parentLocks = new List<IActiveLock>();
            var parentUrl = BuildUrl(destinationUrl);

            foreach (var activeLock in _locks.Values)
            {
                var lockUrl = BuildUrl(activeLock.Path);
                var result = Compare(parentUrl, withChildren, lockUrl, activeLock.Recursive);
                switch (result)
                {
                    case LockCompareResult.Reference:
                        refLocks.Add(activeLock);
                        break;
                    case LockCompareResult.LeftIsParent:
                        childLocks.Add(activeLock);
                        break;
                    case LockCompareResult.RightIsParent:
                        parentLocks.Add(activeLock);
                        break;
                }
            }

            return new LockStatus(refLocks, parentLocks, childLocks);
        }

        private LockCompareResult Compare(Uri left, bool leftRecursive, Uri right, bool rightRecursive)
        {
            if (left == right)
            {
                return LockCompareResult.Reference;
            }

            if (left.IsBaseOf(right) && leftRecursive)
            {
                return LockCompareResult.LeftIsParent;
            }

            if (right.IsBaseOf(left) && rightRecursive)
            {
                return LockCompareResult.RightIsParent;
            }

            return LockCompareResult.NoMatch;
        }

        private Uri BuildUrl(Uri relativeUrl)
        {
            return BuildUrl(relativeUrl.OriginalString);
        }

        private Uri BuildUrl(string path)
        {
            return new Uri(_baseUrl, path + (path.EndsWith("/") ? string.Empty : "/"));
        }
    }
}
