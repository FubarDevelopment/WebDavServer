// <copyright file="IImplicitLock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// The interface for the result of the <code>If</code> header evaluation
    /// </summary>
    public interface IImplicitLock
    {
        /// <summary>
        /// Gets the locks matched by the <code>If</code> header or implicit shared lock
        /// </summary>
        IReadOnlyCollection<IActiveLock> OwnedLocks { get; }

        /// <summary>
        /// Gets the locks preventing the usage of an implicit lock
        /// </summary>
        IReadOnlyCollection<IActiveLock> ConflictingLocks { get; }

        /// <summary>
        /// Gets a value indicating whether an implicit lock was created
        /// </summary>
        bool IsTemporaryLock { get; }

        /// <summary>
        /// Gets a value indicating whether the <code>If</code> header was evaluated successfully
        /// </summary>
        bool IsSuccessful { get; }

        /// <summary>
        /// Disposes the temporary lock (when it was issued)
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The async task</returns>
        Task DisposeAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Creates an error response when <see cref="IsSuccessful"/> is <see langref="false"/>
        /// </summary>
        /// <returns>The WebDAV response</returns>
        IWebDavResult CreateErrorResponse();
    }
}
