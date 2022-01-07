// <copyright file="ImplicitLock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Models;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// Implementation of the <see cref="IImplicitLock"/> interface.
    /// </summary>
    public class ImplicitLock : IImplicitLock
    {
        private readonly ILockManager? _lockManager;
        private readonly IReadOnlyCollection<IActiveLock>? _matchedLocks;
        private readonly IReadOnlyCollection<IActiveLock>? _conflictingLocks;
        private readonly IActiveLock? _acquiredLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImplicitLock"/> class.
        /// </summary>
        /// <param name="isSuccess"><see langword="false"/> = All <c>If</c> header conditions failed,
        /// <see langword="true"/> = No lock manager, but still OK.</param>
        public ImplicitLock(bool isSuccess = false)
        {
            IsSuccessful = isSuccess;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImplicitLock"/> class.
        /// </summary>
        /// <param name="matchedLocks">The locks matched by the <c>If</c> header.</param>
        public ImplicitLock(IReadOnlyCollection<IActiveLock> matchedLocks)
        {
            _matchedLocks = matchedLocks;
            IsSuccessful = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImplicitLock"/> class.
        /// </summary>
        /// <param name="lockManager">The lock manager.</param>
        /// <param name="acquiredLock">The implicit lock.</param>
        public ImplicitLock(ILockManager lockManager, IActiveLock acquiredLock)
        {
            _lockManager = lockManager;
            _acquiredLock = acquiredLock;
            IsSuccessful = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImplicitLock"/> class.
        /// </summary>
        /// <param name="conflictingLocks">The collection of locks preventing locking the given destination.</param>
        public ImplicitLock(LockStatus conflictingLocks)
        {
            _conflictingLocks = conflictingLocks.GetLocks().ToList();
            IsSuccessful = false;
        }

        /// <inheritdoc />
        public IActiveLock AcquiredLock =>
            _acquiredLock ?? throw new InvalidOperationException("No implicit lock taken.");

        /// <inheritdoc />
        public IReadOnlyCollection<IActiveLock> MatchedLocks =>
            _matchedLocks ?? Array.Empty<IActiveLock>();

        /// <inheritdoc />
        public IReadOnlyCollection<IActiveLock> ConflictingLocks =>
            _conflictingLocks ?? Array.Empty<IActiveLock>();

        /// <inheritdoc />
        public bool IsAcquiredLock => _acquiredLock != null;

        /// <inheritdoc />
        public bool IsSuccessful { get; }

        /// <inheritdoc />
        public IWebDavResult CreateErrorResponse()
        {
            if (IsSuccessful)
            {
                throw new InvalidOperationException("No error to create a response for.");
            }

            // An "If" header condition succeeded, but we couldn't find a matching lock.
            // Obtaining a temporary lock failed.
            var error = new error()
            {
                ItemsElementName = new[] { ItemsChoiceType.locktokensubmitted, },
                Items = new object[]
                {
                    new errorLocktokensubmitted()
                    {
                        href = ConflictingLocks.Select(x => x.Href).ToArray(),
                    },
                },
            };

            return new WebDavResult<error>(WebDavStatusCode.Locked, error);
        }

        /// <inheritdoc />
        public Task DisposeAsync(CancellationToken cancellationToken)
        {
            if (_acquiredLock == null || _lockManager == null)
            {
                return Task.CompletedTask;
            }

            // A temporary lock is always on its own
            var l = _acquiredLock;

            // Ignore errors, because the only error that may happen
            // is, that the lock already expired.
            return _lockManager.ReleaseAsync(l.Path, new Uri(l.StateToken, UriKind.RelativeOrAbsolute), cancellationToken);
        }
    }
}
