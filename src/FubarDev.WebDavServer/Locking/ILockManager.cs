// <copyright file="ILockManager.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using LanguageExt;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// The interface for a lock manager
    /// </summary>
    public interface ILockManager
    {
        /// <summary>
        /// Tries to issue a lock
        /// </summary>
        /// <param name="l">The lock to issue</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>Either the list of locks preventing issuing a lock or the active lock created</returns>
        Task<Either<IReadOnlyCollection<IActiveLock>, IActiveLock>> LockAsync(ILock l, CancellationToken cancellationToken);

        /// <summary>
        /// Releases a lock with the given state token
        /// </summary>
        /// <param name="stateToken">The state token of the lock to release</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns><see langword="true" /> when there was a lock to remove</returns>
        Task<bool> ReleaseAsync(Uri stateToken, CancellationToken cancellationToken);

        /// <summary>
        /// Gets all active locks
        /// </summary>
        /// <remarks>
        /// Be aware that the locks could've been released in the mean time by a concurrent
        /// access or by the <see cref="LockCleanupTask"/>.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>Returns all active locks</returns>
        Task<IEnumerable<IActiveLock>> GetLocksAsync(CancellationToken cancellationToken);
    }
}
