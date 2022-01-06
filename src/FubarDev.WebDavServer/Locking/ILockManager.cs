// <copyright file="ILockManager.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Models;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// The interface for a lock manager
    /// </summary>
    public interface ILockManager
    {
        /// <summary>
        /// Gets called when a lock was added
        /// </summary>
        event EventHandler<LockEventArgs>? LockAdded;

        /// <summary>
        /// Gets called when a lock was released
        /// </summary>
        event EventHandler<LockEventArgs>? LockReleased;

        /// <summary>
        /// Gets the cost of a LOCK/UNLOCK or lock discovery operation.
        /// </summary>
        int Cost { get; }

        /// <summary>
        /// Tries to issue a lock.
        /// </summary>
        /// <param name="requestedLock">The lock to issue.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Either the list of locks preventing issuing a lock or the active lock created.</returns>
        Task<LockResult> LockAsync(ILock requestedLock, CancellationToken cancellationToken = default);

        /// <summary>
        /// Tries to find a lock identified by the <paramref name="ifHeaderLists"/> or creates a new one if none was found.
        /// </summary>
        /// <param name="rootFileSystem">The root file system to identify the lock for.</param>
        /// <param name="ifHeaderLists">The <c>If</c> header lists that tries to identify the lock to use.</param>
        /// <param name="lockRequirements">The requirements the found lock must meet.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Either the list of locks preventing issuing a lock or the active lock created or reused.</returns>
        Task<IImplicitLock> LockImplicitAsync(
            IFileSystem rootFileSystem,
            IReadOnlyCollection<IfHeader>? ifHeaderLists,
            ILock lockRequirements,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tries to refresh a lock.
        /// </summary>
        /// <param name="rootFileSystem">The root file system.</param>
        /// <param name="targetPath">The target path to refresh the lock token for.</param>
        /// <param name="ifHeader">The header that tries to identify the lock to refresh.</param>
        /// <param name="timeout">The header containing the new timeouts.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Either the list of locks preventing refreshing a lock or the refreshed lock.</returns>
        Task<LockRefreshResult> RefreshLockAsync(
            IFileSystem rootFileSystem,
            string targetPath,
            IfHeader ifHeader,
            TimeSpan timeout,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Releases a lock with the given state token.
        /// </summary>
        /// <param name="path">The path to release the lock for.</param>
        /// <param name="stateToken">The state token of the lock to release.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><see langword="true" /> when there was a lock to remove.</returns>
        Task<LockReleaseStatus> ReleaseAsync(
            string path,
            Uri stateToken,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all active locks.
        /// </summary>
        /// <remarks>
        /// Be aware that the locks could've been released in the mean time by a concurrent
        /// access or by the <see cref="LockCleanupTask"/>.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns all active locks.</returns>
        Task<IEnumerable<IActiveLock>> GetLocksAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all active locks.
        /// </summary>
        /// <remarks>
        /// Be aware that the locks could've been released in the mean time by a concurrent
        /// access or by the <see cref="LockCleanupTask"/>.
        /// </remarks>
        /// <param name="path">The file system path to get the locks for.</param>
        /// <param name="findChildren">Indicates whether all locks that are a child of the given <paramref name="path"/> should be returned.</param>
        /// <param name="findParents">Indicates whether all locks that are a parent of the given <paramref name="path"/> should be returned.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns all active locks.</returns>
        Task<IEnumerable<IActiveLock>> GetAffectedLocksAsync(
            string path,
            bool findChildren,
            bool findParents,
            CancellationToken cancellationToken = default);
    }
}
