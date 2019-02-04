// <copyright file="LockRefreshResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// The result of a LOCK refresh operation.
    /// </summary>
    public class LockRefreshResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockRefreshResult"/> class.
        /// </summary>
        /// <param name="errorResponse">The error to return.</param>
        public LockRefreshResult(response errorResponse)
        {
            ErrorResponse = errorResponse;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockRefreshResult"/> class.
        /// </summary>
        /// <param name="refreshedLocks">The active locks that could be refreshed.</param>
        public LockRefreshResult(IReadOnlyCollection<IActiveLock> refreshedLocks)
        {
            RefreshedLocks = refreshedLocks;
        }

        /// <summary>
        /// Gets the active lock when locking succeeded.
        /// </summary>
        [CanBeNull]
        public IReadOnlyCollection<IActiveLock> RefreshedLocks { get; }

        /// <summary>
        /// Gets the error response to return.
        /// </summary>
        [CanBeNull]
        public response ErrorResponse { get; }
    }
}
