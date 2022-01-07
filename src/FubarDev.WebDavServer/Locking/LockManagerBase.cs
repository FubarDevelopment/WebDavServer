// <copyright file="LockManagerBase.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Models;
using FubarDev.WebDavServer.Utils;

using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Locking;

/// <summary>
/// The base implementation for an <see cref="ILockManager"/>
/// </summary>
/// <remarks>
/// The derived class must implement <see cref="BeginTransactionAsync"/> and
/// return an object that implements <see cref="ILockManagerTransaction"/>.
/// </remarks>
public abstract class LockManagerBase : ILockManager
{
    private static readonly Uri _baseUrl = new Uri("http://localhost/");

    private readonly IWebDavContextAccessor _contextAccessor;

    private readonly ILockCleanupTask _cleanupTask;

    private readonly ISystemClock _systemClock;

    private readonly ILogger _logger;

    private readonly ILockTimeRounding _rounding;

    /// <summary>
    /// Initializes a new instance of the <see cref="LockManagerBase"/> class.
    /// </summary>
    /// <param name="contextAccessor">The WebDAV context accessor.</param>
    /// <param name="cleanupTask">The clean-up task for expired locks.</param>
    /// <param name="systemClock">The system clock interface.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The options of the lock manager.</param>
    protected LockManagerBase(
        IWebDavContextAccessor contextAccessor,
        ILockCleanupTask cleanupTask,
        ISystemClock systemClock,
        ILogger logger,
        ILockManagerOptions? options = null)
    {
        _rounding = options?.Rounding ?? new DefaultLockTimeRounding(DefaultLockTimeRoundingMode.OneSecond);
        _contextAccessor = contextAccessor;
        _cleanupTask = cleanupTask;
        _systemClock = systemClock;
        _logger = logger;
    }

    /// <inheritdoc />
    public event EventHandler<LockEventArgs>? LockAdded;

    /// <inheritdoc />
    public event EventHandler<LockEventArgs>? LockReleased;

    private enum LockCompareResult
    {
        RightIsParent,
        LeftIsParent,
        Reference,
        NoMatch,
    }

    /// <summary>
    /// This interface must be implemented by the inheriting class.
    /// </summary>
    protected interface ILockManagerTransaction : IDisposable
    {
        /// <summary>
        /// Gets all active locks.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The collection of all active locks.</returns>
        Task<IReadOnlyCollection<IActiveLock>> GetActiveLocksAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Adds a new active lock.
        /// </summary>
        /// <param name="activeLock">The active lock to add.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><see langword="true"/> when adding the lock succeeded.</returns>
        Task<bool> AddAsync(IActiveLock activeLock, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the active lock.
        /// </summary>
        /// <param name="activeLock">The active lock with the updated values.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><see langword="true"/> when the lock was updated, <see langword="false"/> when the lock was added instead.</returns>
        Task<bool> UpdateAsync(IActiveLock activeLock, CancellationToken cancellationToken);

        /// <summary>
        /// Removes an active lock with the given <paramref name="stateToken"/>.
        /// </summary>
        /// <param name="stateToken">The state token of the active lock to remove.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><see langword="true"/> when a lock with the given <paramref name="stateToken"/> existed and could be removed.</returns>
        Task<bool> RemoveAsync(string stateToken, CancellationToken cancellationToken);

        /// <summary>
        /// Gets an active lock by its <paramref name="stateToken"/>.
        /// </summary>
        /// <param name="stateToken">The state token to search for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The active lock for the state token or <see langword="null"/> when the lock wasn't found.</returns>
        Task<IActiveLock?> GetAsync(string stateToken, CancellationToken cancellationToken);

        /// <summary>
        /// Commits the changes made during the transaction.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The async task.</returns>
        Task CommitAsync(CancellationToken cancellationToken);
    }

    /// <inheritdoc />
    public int Cost { get; } = 0;

    /// <summary>
    /// Gets the lock cleanup task.
    /// </summary>
    protected ILockCleanupTask LockCleanupTask => _cleanupTask;

    /// <inheritdoc />
    public async Task<LockResult> LockAsync(ILock requestedLock, CancellationToken cancellationToken)
    {
        ActiveLock newActiveLock;
        var destinationUrl = BuildUrl(requestedLock.Path);
        using (var transaction = await BeginTransactionAsync(cancellationToken).ConfigureAwait(false))
        {
            var locks = await transaction.GetActiveLocksAsync(cancellationToken).ConfigureAwait(false);
            var status = Find(locks, destinationUrl, requestedLock.Recursive, true);
            var conflictingLocks = GetConflictingLocks(status, requestedLock);
            if (conflictingLocks.Count != 0)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation(
                        "Found conflicting locks for {Lock}: {ConflictingLocks}",
                        requestedLock,
                        string.Join(",", conflictingLocks.GetLocks().Select(x => x.ToString())));
                }

                return new LockResult(conflictingLocks);
            }

            newActiveLock = new ActiveLock(requestedLock, _rounding.Round(_systemClock.UtcNow), _rounding.Round(requestedLock.Timeout));

            await transaction.AddAsync(newActiveLock, cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        OnLockAdded(newActiveLock);

        _cleanupTask.Add(this, newActiveLock);

        return new LockResult(newActiveLock);
    }

    /// <inheritdoc />
    public async Task<IImplicitLock> LockImplicitAsync(
        IFileSystem rootFileSystem,
        IReadOnlyCollection<IfHeader>? ifHeaderLists,
        ILock lockRequirements,
        CancellationToken cancellationToken)
    {
        if (ifHeaderLists == null || ifHeaderLists.Count == 0)
        {
            var newLock = await LockAsync(lockRequirements, cancellationToken).ConfigureAwait(false);
            return newLock.IsSuccess
                ? new ImplicitLock(this, newLock.Lock)
                : new ImplicitLock(newLock.ConflictingLocks);
        }

        var unmatchedLocks = await FindUnmatchedLocksAsync(
            rootFileSystem,
            ifHeaderLists,
            lockRequirements,
            cancellationToken).ConfigureAwait(false);
        if (unmatchedLocks.ConflictCount != 0)
        {
            return new ImplicitLock(
                new LockStatus(
                    unmatchedLocks.ReferenceLocks,
                    unmatchedLocks.ParentLocks,
                    unmatchedLocks.ChildLocks));
        }

        if (unmatchedLocks.MatchedLocks.Count == 0)
        {
            // No unmatched locks found
            var newLock = await LockAsync(lockRequirements, cancellationToken).ConfigureAwait(false);
            return newLock.IsSuccess
                ? new ImplicitLock(this, newLock.Lock)
                : new ImplicitLock(newLock.ConflictingLocks);
        }

        return new ImplicitLock(unmatchedLocks.MatchedLocks.Keys.ToList());
    }

    /// <inheritdoc />
    public async Task<LockRefreshResult> RefreshLockAsync(
        IFileSystem rootFileSystem,
        string targetPath,
        IfHeader ifHeader,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var context = _contextAccessor.WebDavContext;
        var ifHeaderMatcher = new IfHeaderMatcher(
            context,
            rootFileSystem,
            targetPath,
            new[] { ifHeader });

        var failedHrefs = new HashSet<Uri>(
            context.GetHrefOrResourceTag(ifHeader));
        var refreshedLocks = new List<ActiveLock>();

        using (var transaction = await BeginTransactionAsync(cancellationToken).ConfigureAwait(false))
        {
            var activeLocks = await transaction.GetActiveLocksAsync(cancellationToken).ConfigureAwait(false);
            var matches = await FindMatchesAsync(
                context.User.Identity,
                ifHeaderMatcher,
                activeLocks,
                cancellationToken);
            foreach (var matchItem in matches)
            {
                var foundLock = matchItem.Key;
                var refreshedLock = Refresh(foundLock, _rounding.Round(_systemClock.UtcNow), _rounding.Round(timeout));

                // Remove old lock from clean-up task
                _cleanupTask.Remove(foundLock);

                refreshedLocks.Add(refreshedLock);

                // Remove HREF from failed list
                foreach (var ifMatch in matchItem.Value)
                {
                    failedHrefs.Remove(context.GetHrefOrResourceTag(ifMatch));
                }
            }

            if (refreshedLocks.Count == 0)
            {
                var hrefs = failedHrefs.ToList();
                var href = hrefs.First().OriginalString;
                var hrefItems = hrefs
                    .Skip(1)
                    .Select(x => x.OriginalString)
                    .Cast<object>()
                    .ToArray();
                var hrefItemNames = hrefItems.Select(_ => ItemsChoiceType2.href).ToArray();

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
                await transaction.UpdateAsync(newLock, cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        foreach (var newLock in refreshedLocks)
        {
            _cleanupTask.Add(this, newLock);
        }

        return new LockRefreshResult(refreshedLocks);
    }

    /// <inheritdoc />
    public async Task<LockReleaseStatus> ReleaseAsync(string path, Uri stateToken, CancellationToken cancellationToken)
    {
        IActiveLock? activeLock;
        using (var transaction = await BeginTransactionAsync(cancellationToken).ConfigureAwait(false))
        {
            activeLock = await transaction.GetAsync(stateToken.OriginalString, cancellationToken).ConfigureAwait(false);
            if (activeLock == null)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation(
                        "Tried to remove non-existent lock {StateToken}",
                        stateToken);
                }

                return LockReleaseStatus.NoLock;
            }

            var destinationUrl = BuildUrl(path);
            var lockUrl = BuildUrl(activeLock.Path);
            var lockCompareResult = Compare(lockUrl, activeLock.Recursive, destinationUrl, false);
            if (lockCompareResult != LockCompareResult.Reference)
            {
                return LockReleaseStatus.InvalidLockRange;
            }

            await transaction.RemoveAsync(stateToken.OriginalString, cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        _cleanupTask.Remove(activeLock);

        OnLockReleased(activeLock);

        return LockReleaseStatus.Success;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IActiveLock>> GetLocksAsync(CancellationToken cancellationToken)
    {
        using var transaction = await BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        var locks = await transaction.GetActiveLocksAsync(cancellationToken).ConfigureAwait(false);
        return locks;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IActiveLock>> GetAffectedLocksAsync(string path, bool findChildren, bool findParents, CancellationToken cancellationToken)
    {
        var destinationUrl = BuildUrl(path);
        LockStatus status;
        using (var transaction = await BeginTransactionAsync(cancellationToken).ConfigureAwait(false))
        {
            var locks = await transaction.GetActiveLocksAsync(cancellationToken).ConfigureAwait(false);
            status = Find(locks, destinationUrl, findChildren, findParents);
        }

        return status.ParentLocks.Concat(status.ReferenceLocks).Concat(status.ChildLocks);
    }

    /// <summary>
    /// Converts a client path to a system path.
    /// </summary>
    /// <remarks>
    /// <para>The client path has the form <c>http://localhost/root-file-system/relative/path</c> and is
    /// therefore always an absolute path. The returned path must be absolute too and might have
    /// the form <c>http://localhost/c/relative/path</c> or something similar. It is of utmost
    /// importance that the URI is always stable. The default implementation of this function
    /// doesn't make any conversions, because it assumes that the same path path always points
    /// to the same file system entry for all clients.</para>
    /// <para>
    /// A URI to a directory must always end in a slash (<c>/</c>).
    /// </para>
    /// </remarks>
    /// <param name="path">The client path to convert</param>
    /// <returns>The system path to be converted to.</returns>
    protected virtual Uri NormalizePath(Uri path)
    {
        return path;
    }

    /// <summary>
    /// Gets called when a lock was added.
    /// </summary>
    /// <param name="activeLock">The lock that was added.</param>
    protected virtual void OnLockAdded(IActiveLock activeLock)
    {
        LockAdded?.Invoke(this, new LockEventArgs(activeLock));
    }

    /// <summary>
    /// Gets called when a lock was released.
    /// </summary>
    /// <param name="activeLock">The lock that was released.</param>
    protected virtual void OnLockReleased(IActiveLock activeLock)
    {
        LockReleased?.Invoke(this, new LockEventArgs(activeLock));
    }

    /// <summary>
    /// Begins a new transaction.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The transaction to be used to update the active locks.</returns>
    protected abstract Task<ILockManagerTransaction> BeginTransactionAsync(CancellationToken cancellationToken);

    private static LockStatus GetConflictingLocks(LockStatus affectingLocks, ILock l)
    {
        var shareMode = LockShareMode.Parse(l.ShareMode);
        if (shareMode == LockShareMode.Exclusive)
        {
            return affectingLocks;
        }

        return new LockStatus(
            affectingLocks
                .ReferenceLocks
                .Where(x => LockShareMode.Parse(x.ShareMode) == LockShareMode.Exclusive)
                .ToList(),
            affectingLocks
                .ParentLocks
                .Where(x => LockShareMode.Parse(x.ShareMode) == LockShareMode.Exclusive)
                .ToList(),
            affectingLocks
                .ChildLocks
                .Where(x => LockShareMode.Parse(x.ShareMode) == LockShareMode.Exclusive)
                .ToList());
    }

    /// <summary>
    /// Returns a new active lock whose new expiration date/time is recalculated using <paramref name="lastRefresh"/> and <paramref name="timeout"/>.
    /// </summary>
    /// <param name="activeLock">The active lock to refresh.</param>
    /// <param name="lastRefresh">The date/time of the last refresh.</param>
    /// <param name="timeout">The new timeout to apply to the lock.</param>
    /// <returns>The new (refreshed) active lock.</returns>
    [Pure]
    private static ActiveLock Refresh(IActiveLock activeLock, DateTime lastRefresh, TimeSpan timeout)
    {
        return new ActiveLock(
            activeLock.Path,
            activeLock.Href,
            activeLock.Recursive,
            activeLock.Owner,
            activeLock.GetOwnerHref(),
            LockAccessType.Parse(activeLock.AccessType),
            LockShareMode.Parse(activeLock.ShareMode),
            timeout,
            activeLock.Issued,
            lastRefresh,
            activeLock.StateToken);
    }

    [Pure]
    private static async ValueTask<IReadOnlyDictionary<IActiveLock, IReadOnlyCollection<IfHeaderMatch>>> FindMatchesAsync(
        IIdentity? identity,
        IfHeaderMatcher ifHeaderMatcher,
        IEnumerable<IActiveLock> activeLocks,
        CancellationToken cancellationToken)
    {
        Dictionary<IActiveLock, IReadOnlyCollection<IfHeaderMatch>> result = new();
        foreach (var activeLock in activeLocks)
        {
            // Cannot use a lock token for a different owner
            if (!identity.IsSameOwner(activeLock.Owner))
            {
                continue;
            }

            var matches = await ifHeaderMatcher.FindAsync(
                    new[] { new Uri(activeLock.StateToken) },
                    cancellationToken)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            if (matches.Count != 0)
            {
                result.Add(activeLock, matches);
            }
        }

        return result;
    }

    private async Task<LockMatch> FindUnmatchedLocksAsync(
        IFileSystem rootFileSystem,
        IReadOnlyCollection<IfHeader> ifHeaderLists,
        ILock lockRequirements,
        CancellationToken cancellationToken)
    {
        var lockRequirementUrl = BuildUrl(lockRequirements.Path);

        LockStatus affectingLocks;
        using (var transaction = await BeginTransactionAsync(cancellationToken).ConfigureAwait(false))
        {
            var locks = await transaction.GetActiveLocksAsync(cancellationToken).ConfigureAwait(false);
            affectingLocks = Find(locks, lockRequirementUrl, lockRequirements.Recursive, true);
        }

        var context = _contextAccessor.WebDavContext;
        var ifHeaderMatcher = new IfHeaderMatcher(
            context,
            rootFileSystem,
            lockRequirements.Path,
            ifHeaderLists);

        var ifHeaderMatches = await FindMatchesAsync(
                context.User.Identity,
                ifHeaderMatcher,
                affectingLocks.GetLocks(),
                cancellationToken)
            .ConfigureAwait(false);

        var matchedLocks = ifHeaderMatches.Keys.ToImmutableHashSet();
        var unmatchedLocks = new LockMatch(
            ifHeaderMatches,
            affectingLocks.ReferenceLocks.Where(x => !matchedLocks.Contains(x)).ToList(),
            affectingLocks.ParentLocks.Where(x => !matchedLocks.Contains(x)).ToList(),
            affectingLocks.ChildLocks.Where(x => !matchedLocks.Contains(x)).ToList());

        return unmatchedLocks;
    }

    private LockStatus Find(IEnumerable<IActiveLock> locks, Uri parentUrl, bool withChildren, bool findParents)
    {
        var normalizedParentUrl = NormalizePath(parentUrl);
        var refLocks = new List<IActiveLock>();
        var childLocks = new List<IActiveLock>();
        var parentLocks = new List<IActiveLock>();

        foreach (var activeLock in locks)
        {
            var lockUrl = BuildUrl(activeLock.Path);
            var normalizedLockUrl = NormalizePath(lockUrl);
            var result = Compare(normalizedParentUrl, withChildren, normalizedLockUrl, activeLock.Recursive);
            switch (result)
            {
                case LockCompareResult.Reference:
                    refLocks.Add(activeLock);
                    break;
                case LockCompareResult.LeftIsParent:
                    childLocks.Add(activeLock);
                    break;
                case LockCompareResult.RightIsParent:
                    if (findParents)
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
        {
            return _baseUrl;
        }

        return new Uri(_baseUrl, path + (path.EndsWith("/") ? string.Empty : "/"));
    }

    private class LockMatch
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockMatch"/> class.
        /// </summary>
        /// <param name="matchedLocks">The found (matched) locks.</param>
        /// <param name="referenceLocks">The locks found at the reference position.</param>
        /// <param name="parentLocks">The locks found at positions higher in the hierarchy.</param>
        /// <param name="childLocks">The locks found at positions lower in the hierarchy.</param>
        public LockMatch(
            IReadOnlyDictionary<IActiveLock, IReadOnlyCollection<IfHeaderMatch>> matchedLocks,
            IReadOnlyCollection<IActiveLock> referenceLocks,
            IReadOnlyCollection<IActiveLock> parentLocks,
            IReadOnlyCollection<IActiveLock> childLocks)
        {
            MatchedLocks = matchedLocks;
            ReferenceLocks = referenceLocks;
            ParentLocks = parentLocks;
            ChildLocks = childLocks;
        }

        /// <summary>
        /// Gets the matched locks.
        /// </summary>
        public IReadOnlyDictionary<IActiveLock, IReadOnlyCollection<IfHeaderMatch>> MatchedLocks { get; }

        /// <summary>
        /// Gets the locks found at the reference position.
        /// </summary>
        public IReadOnlyCollection<IActiveLock> ReferenceLocks { get; }

        /// <summary>
        /// Gets the locks found at positions higher in the hierarchy.
        /// </summary>
        public IReadOnlyCollection<IActiveLock> ParentLocks { get; }

        /// <summary>
        /// Gets the locks found at positions lower in the hierarchy.
        /// </summary>
        public IReadOnlyCollection<IActiveLock> ChildLocks { get; }

        /// <summary>
        /// Gets the number of found locks.
        /// </summary>
        public int ConflictCount => ReferenceLocks.Count + ParentLocks.Count + ChildLocks.Count;
    }
}
