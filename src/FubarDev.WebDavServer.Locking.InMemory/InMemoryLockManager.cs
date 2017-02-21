// <copyright file="InMemoryLockManager.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

#define USE_VARIANT_2

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;

using JetBrains.Annotations;

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
        public async Task<ImplicitLock> LockImplicitAsync(
            IFileSystem rootFileSystem,
            IReadOnlyCollection<IfHeaderList> ifHeaderLists,
            ILock lockRequirements,
            CancellationToken cancellationToken)
        {
            if (ifHeaderLists == null || ifHeaderLists.Count == 0)
            {
                var newLock = await LockAsync(lockRequirements, cancellationToken).ConfigureAwait(false);
                return new ImplicitLock(this, newLock);
            }

            var successfulConditions = await FindMatchingIfConditionList(
                rootFileSystem,
                ifHeaderLists,
                lockRequirements,
                cancellationToken).ConfigureAwait(false);
            if (successfulConditions == null)
            {
                // No if conditions found for the requested path
                var newLock = await LockAsync(lockRequirements, cancellationToken).ConfigureAwait(false);
                return new ImplicitLock(this, newLock);
            }

            var firstConditionWithStateToken = successfulConditions.FirstOrDefault(x => x.Item2.RequiresStateToken);
            if (firstConditionWithStateToken != null)
            {
                // Returns the list of locks matched by the first if list
                var usedLocks = firstConditionWithStateToken
                    .Item2.Conditions.Where(x => x.StateToken != null && !x.Not)
                    .Select(x => firstConditionWithStateToken.Item1.TokenToLock[x.StateToken]).ToList();
                return new ImplicitLock(usedLocks);
            }

            if (successfulConditions.Count != 0)
            {
                // At least one "If" header condition was successful, but we didn't find any with a state token
                var newLock = await LockAsync(lockRequirements, cancellationToken).ConfigureAwait(false);
                return new ImplicitLock(this, newLock);
            }

            return new ImplicitLock();
        }

        /// <inheritdoc />
        public async Task<LockRefreshResult> RefreshLockAsync(IFileSystem rootFileSystem, IfHeader ifHeader, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var failedHrefs = new HashSet<Uri>();
            var refreshedLocks = new List<ActiveLock>();

            var pathToInfo = new Dictionary<Uri, PathInfo>();
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
                            continue;
                        }

                        pathInfo.EntityTag = await selectionResult.TargetEntry.GetEntityTagAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            lock (_syncRoot)
            {
                foreach (var ifHeaderList in ifHeader.Lists.Where(x => x.RequiresStateToken))
                {
                    var pathInfo = pathToInfo[ifHeaderList.Path];

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
                        pathInfo.LockTokens = pathInfo.TokenToLock.Keys.ToList();
                    }

                    var foundLock = pathInfo.TokenToLock.Where(x => ifHeaderList.IsMatch(pathInfo.EntityTag, new[] { x.Key })).Select(x => x.Value).SingleOrDefault();
                    if (foundLock != null)
                    {
                        var refreshedLock = foundLock.Refresh(_rounding.Round(_systemClock.UtcNow), _rounding.Round(timeout));

                        // Remove old lock from clean-up task
                        _cleanupTask.Remove(foundLock);

                        refreshedLocks.Add(refreshedLock);
                    }
                    else
                    {
                        failedHrefs.Add(ifHeaderList.RelativeHref);
                    }
                }

                if (refreshedLocks.Count == 0)
                {
                    var hrefs = failedHrefs.ToList();
                    var href = hrefs.First().OriginalString;
                    var hrefItems = hrefs.Skip(1).Select(x => x.OriginalString).Cast<object>().ToArray();
                    var hrefItemNames = hrefItems.Select(x => ItemsChoiceType2.href).ToArray();

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

                foreach (var newLock in refreshedLocks)
                {
                    var stateToken = new Uri(newLock.StateToken);
                    _locks = _locks
                        .Remove(stateToken)
                        .Add(stateToken, newLock);
                }
            }

            foreach (var newLock in refreshedLocks)
            {
                // Add refreshed lock to the clean-up task
                _cleanupTask.Add(this, newLock);
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
            lock (_syncRoot)
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

#if USE_VARIANT_1
        [NotNull]
        [ItemCanBeNull]
        private async Task<IReadOnlyCollection<Tuple<PathInfo, IfHeaderList>>> FindMatchingIfConditionList(
            [NotNull] IFileSystem rootFileSystem,
            [NotNull] [ItemNotNull] IReadOnlyCollection<IfHeaderList> ifHeaderLists,
            [NotNull] ILock lockRequirements,
            CancellationToken cancellationToken)
        {
            var lockRequirementUrl = BuildUrl(lockRequirements.Path);

            var supportedIfConditions = new List<IfHeaderList>();
            var pathToInfo = new Dictionary<Uri, PathInfo>();
            foreach (var ifHeaderList in ifHeaderLists)
            {
                var ifHeaderUrl = BuildUrl(ifHeaderList.Path.OriginalString);
                var headerCompareResult = Compare(ifHeaderUrl, true, lockRequirementUrl, false);
                if (headerCompareResult != LockCompareResult.LeftIsParent &&
                    headerCompareResult != LockCompareResult.Reference)
                    continue;

                supportedIfConditions.Add(ifHeaderList);

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
                        var selectionResult = await rootFileSystem
                            .SelectAsync(ifHeaderList.Path.OriginalString, cancellationToken).ConfigureAwait(false);
                        if (selectionResult.IsMissing)
                        {
                            // Probably locked entry not found
                            continue;
                        }

                        pathInfo.EntityTag = await selectionResult
                            .TargetEntry.GetEntityTagAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            if (supportedIfConditions.Count == 0)
                return null;

            var successfulConditions = new List<Tuple<PathInfo, IfHeaderList>>();
            lock (_syncRoot)
            {
                foreach (var ifHeaderList in supportedIfConditions)
                {
                    var pathInfo = pathToInfo[ifHeaderList.Path];

                    if (pathInfo.ActiveLocks == null)
                    {
                        var destinationUrl = BuildUrl(ifHeaderList.Path.OriginalString);
                        var entryLocks = Find(destinationUrl, false).GetLocks().Cast<ActiveLock>().ToList();
                        pathInfo.ActiveLocks = entryLocks;
                        pathInfo.TokenToLock = entryLocks.ToDictionary(x => new Uri(x.StateToken, UriKind.RelativeOrAbsolute));
                        pathInfo.LockTokens = pathInfo.TokenToLock.Keys.ToList();
                    }

                    if (ifHeaderList.IsMatch(pathInfo.EntityTag, pathInfo.LockTokens))
                    {
                        successfulConditions.Add(Tuple.Create(pathInfo, ifHeaderList));
                    }
                }
            }

            return successfulConditions;
        }
#endif

#if USE_VARIANT_2
        [NotNull]
        [ItemCanBeNull]
        private async Task<IReadOnlyCollection<Tuple<PathInfo, IfHeaderList>>> FindMatchingIfConditionList(
            [NotNull] IFileSystem rootFileSystem,
            [NotNull] [ItemNotNull] IReadOnlyCollection<IfHeaderList> ifHeaderLists,
            [NotNull] ILock lockRequirements,
            CancellationToken cancellationToken)
        {
            var lockRequirementUrl = BuildUrl(lockRequirements.Path);

            IReadOnlyCollection<ActiveLock> affectingLocks;
            lock (_syncRoot)
            {
                var lockStatus = Find(lockRequirementUrl, false);
                affectingLocks = lockStatus.ParentLocks.Concat(lockStatus.ReferenceLocks).Cast<ActiveLock>().ToList();
            }

            // Get all If header lists together with all relevant active locks
            var ifListLocks =
                (from list in ifHeaderLists
                 let listUrl = BuildUrl(list.Path.OriginalString)
                 let compareResult = Compare(listUrl, true, lockRequirementUrl, false)
                 where compareResult == LockCompareResult.LeftIsParent
                       || compareResult == LockCompareResult.Reference
                 let foundLocks = list.RequiresStateToken
                     ? Find(affectingLocks, listUrl, compareResult == LockCompareResult.LeftIsParent)
                     : LockStatus.Empty
                 let locksForIfConditions = foundLocks.GetLocks().Cast<ActiveLock>().ToList()
                 select Tuple.Create<IfHeaderList, IReadOnlyCollection<ActiveLock>>(list, locksForIfConditions))
                .ToDictionary(x => x.Item1, x => x.Item2);

            // List of matches between path info and if header lists
            var successfulConditions = new List<Tuple<PathInfo, IfHeaderList>>();
            if (ifListLocks.Count == 0)
                return null;

            // Collect all file system specific information
            var pathToInfo = new Dictionary<Uri, PathInfo>();
            foreach (var matchingIfListItem in ifListLocks)
            {
                var ifHeaderList = matchingIfListItem.Key;
                PathInfo pathInfo;
                if (!pathToInfo.TryGetValue(ifHeaderList.Path, out pathInfo))
                {
                    pathInfo = new PathInfo();
                    pathInfo.ActiveLocks = matchingIfListItem.Value;
                    pathInfo.TokenToLock = matchingIfListItem
                        .Value.ToDictionary(x => new Uri(x.StateToken, UriKind.RelativeOrAbsolute));
                    pathInfo.LockTokens = pathInfo.TokenToLock.Keys.ToList();
                    pathToInfo.Add(ifHeaderList.Path, pathInfo);
                }

                if (pathInfo.EntityTag == null)
                {
                    if (ifHeaderList.RequiresEntityTag)
                    {
                        var selectionResult = await rootFileSystem
                            .SelectAsync(ifHeaderList.Path.OriginalString, cancellationToken).ConfigureAwait(false);
                        if (!selectionResult.IsMissing)
                        {
                            pathInfo.EntityTag = await selectionResult
                                .TargetEntry.GetEntityTagAsync(cancellationToken)
                                .ConfigureAwait(false);
                        }
                    }
                }

                if (ifHeaderList.IsMatch(pathInfo.EntityTag, pathInfo.LockTokens))
                {
                    successfulConditions.Add(Tuple.Create(pathInfo, ifHeaderList));
                }
            }

            return successfulConditions;
        }
#endif

        private LockStatus Find(Uri parentUrl, bool withChildren)
        {
            return Find(_locks.Values, parentUrl, withChildren);
        }

        private LockStatus Find(IEnumerable<IActiveLock> locks, Uri parentUrl, bool withChildren)
        {
            var refLocks = new List<IActiveLock>();
            var childLocks = new List<IActiveLock>();
            var parentLocks = new List<IActiveLock>();

            foreach (var activeLock in locks)
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
            if (string.IsNullOrEmpty(path))
                return _baseUrl;
            return new Uri(_baseUrl, path + (path.EndsWith("/") ? string.Empty : "/"));
        }

        private class PathInfo
        {
            public EntityTag? EntityTag { get; set; }

            public IReadOnlyCollection<IActiveLock> ActiveLocks { get; set; }

            public IDictionary<Uri, ActiveLock> TokenToLock { get; set; }

            public IReadOnlyCollection<Uri> LockTokens { get; set; }
        }
    }
}
