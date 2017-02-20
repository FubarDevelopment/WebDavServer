// <copyright file="InMemoryLockManager.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;

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

        private IImmutableDictionary<Uri, ActiveLock> _locks = ImmutableDictionary<Uri, ActiveLock>.Empty;

        public InMemoryLockManager(IOptions<LockManagerOptions> options, LockCleanupTask cleanupTask, ISystemClock systemClock, ILogger<InMemoryLockManager> logger)
        {
            var opt = options?.Value ?? new LockManagerOptions();
            _rounding = opt.Rounding ?? new DefaultLockTimeRounding(DefaultLockTimeRoundingMode.OneSecond);
            _cleanupTask = cleanupTask;
            _systemClock = systemClock;
            _logger = logger;
        }

        /// <inheritdoc />
        public event EventHandler<LockEventArgs> LockAdded;

        /// <inheritdoc />
        public event EventHandler<LockEventArgs> LockReleased;

        private enum LockCompareResult
        {
            RightIsParent,
            LeftIsParent,
            Reference,
            NoMatch,
        }

        /// <inheritdoc />
        public int Cost { get; } = 0;

        /// <inheritdoc />
        public Task<LockResult> LockAsync(ILock l, CancellationToken cancellationToken)
        {
            ActiveLock newActiveLock;
            var destinationUrl = BuildUrl(l.Path);
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

        /// <inheritdoc />
        public async Task<LockRefreshResult> RefreshLockAsync(IFileSystem rootFileSystem, IfHeader ifHeader, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var pathToInfo = new Dictionary<Uri, PathInfo>();
            var failedHrefs = new HashSet<Uri>();
            var refreshedLocks = new List<IActiveLock>();

            foreach (var ifHeaderList in ifHeader.Lists.Where(x => x.RequiresStateToken))
            {
                PathInfo pathInfo;
                if (!pathToInfo.TryGetValue(ifHeaderList.Path, out pathInfo))
                {
                    pathInfo = new PathInfo();
                    pathToInfo.Add(ifHeaderList.Path, pathInfo);
                }

                if (pathInfo.EntityTag == null)
                {
                    if (ifHeaderList.RequiresEntityTag)
                    {
                        var selectionResult = await rootFileSystem.SelectAsync(ifHeaderList.Path.OriginalString, cancellationToken).ConfigureAwait(false);
                        if (selectionResult.IsMissing)
                        {
                            // Probably locked entry not found
                            failedHrefs.Add(ifHeaderList.RelativeHref);
                            continue;
                        }

                        pathInfo.EntityTag = await selectionResult.TargetEntry.GetEntityTagAsync(cancellationToken).ConfigureAwait(false);
                    }
                }

                if (pathInfo.ActiveLocks == null)
                {
                    var destinationUrl = BuildUrl(ifHeaderList.Path.OriginalString);
                    var entryLocks = (from l in _locks.Values
                                      let lockUrl = BuildUrl(l.Path)
                                      where Compare(destinationUrl, false, lockUrl, false) == LockCompareResult.Reference
                                      select l).ToList();

                    if (entryLocks.Count == 0)
                    {
                        // No lock found for entry
                        failedHrefs.Add(ifHeaderList.RelativeHref);
                        continue;
                    }

                    pathInfo.ActiveLocks = entryLocks;
                    pathInfo.TokenToLock = entryLocks.ToDictionary(x => new Uri(x.StateToken, UriKind.RelativeOrAbsolute));
                }

                foreach (var tokenToLock in pathInfo.TokenToLock)
                {
                    if (ifHeaderList.IsMatch(pathInfo.EntityTag, new[] { tokenToLock.Key }))
                    {
                        var foundLock = tokenToLock.Value;
                        var refreshedLock = foundLock.Refresh(_rounding.Round(_systemClock.UtcNow), _rounding.Round(timeout));

                        // Remove old lock from clean-up task
                        _cleanupTask.Remove(foundLock);

                        // Add refreshed lock to the clean-up task
                        _cleanupTask.Add(this, refreshedLock);

                        refreshedLocks.Add(foundLock);
                    }
                }
            }

            if (refreshedLocks.Count == 0)
            {
                var hrefs = failedHrefs.ToList();
                var href = hrefs.First().OriginalString;
                var hrefItems = hrefs.Skip(1).Select(x => x.OriginalString).Cast<object>().ToArray();
                var hrefItemNames = hrefs.Select(x => ItemsChoiceType2.href).ToArray();

                return new LockRefreshResult(
                    new response()
                    {
                        href = href,
                        Items = hrefItems,
                        ItemsElementName = hrefItemNames,
                        error = new error()
                        {
                            Items = new[] { new object(), },
                            ItemsElementName = new[] { ItemsChoiceType.locktokenmatchesrequesturi, },
                        },
                    });
            }

            return new LockRefreshResult(refreshedLocks);
        }

        /// <inheritdoc />
        public Task<LockReleaseStatus> ReleaseAsync(string path, Uri stateToken, CancellationToken cancellationToken)
        {
            ActiveLock activeLock;
            lock (_syncRoot)
            {
                if (!_locks.TryGetValue(stateToken, out activeLock))
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation($"Tried to remove non-existent lock {stateToken}");
                    return Task.FromResult(LockReleaseStatus.NoLock);
                }

                var destinationUrl = BuildUrl(path);
                var lockUrl = BuildUrl(activeLock.Path);
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

        /// <inheritdoc />
        public Task<IEnumerable<IActiveLock>> GetLocksAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_locks.Values.Select(x => (IActiveLock)x));
        }

        /// <inheritdoc />
        public Task<IEnumerable<IActiveLock>> GetAffectedLocksAsync(string path, bool recursive, CancellationToken cancellationToken)
        {
            var destinationUrl = BuildUrl(path);
            LockStatus status;
            lock (_locks)
            {
                status = Find(destinationUrl, recursive);
            }

            return Task.FromResult(status.ParentLocks.Concat(status.ReferenceLocks).Concat(status.ChildLocks));
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

        private LockStatus Find(Uri parentUrl, bool withChildren)
        {
            var refLocks = new List<IActiveLock>();
            var childLocks = new List<IActiveLock>();
            var parentLocks = new List<IActiveLock>();

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

        private Uri BuildUrl(string path)
        {
            return new Uri(_baseUrl, path + (path.EndsWith("/") ? string.Empty : "/"));
        }

        private class PathInfo
        {
            public EntityTag? EntityTag { get; set; }

            public IReadOnlyCollection<IActiveLock> ActiveLocks { get; set; }

            public IDictionary<Uri, ActiveLock> TokenToLock { get; set; }
        }
    }
}
