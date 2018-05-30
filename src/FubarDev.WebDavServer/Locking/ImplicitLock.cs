// <copyright file="ImplicitLock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// Implementation of the <see cref="IImplicitLock"/> interface
    /// </summary>
    public class ImplicitLock : IImplicitLock
    {
        private readonly ILockManager _lockManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImplicitLock"/> class.
        /// </summary>
        /// <param name="isSuccess"><see langword="false"/> = All <c>If</c> header conditions failed,
        /// <see langword="true"/> = No lock manager, but still OK</param>
        public ImplicitLock(bool isSuccess = false)
        {
            IsSuccessful = isSuccess;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImplicitLock"/> class.
        /// </summary>
        /// <param name="ownedLocks">The locks matched by the <c>If</c> header</param>
        public ImplicitLock([NotNull] [ItemNotNull] IReadOnlyCollection<IActiveLock> ownedLocks)
        {
            OwnedLocks = ownedLocks;
            IsSuccessful = true;
            IsTemporaryLock = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImplicitLock"/> class.
        /// </summary>
        /// <param name="lockManager">The lock manager</param>
        /// <param name="lockResult">Either the implicit lock or the conflicting locks</param>
        public ImplicitLock([NotNull] ILockManager lockManager, [NotNull] LockResult lockResult)
        {
            _lockManager = lockManager;
            if (lockResult.Lock != null)
                OwnedLocks = new[] { lockResult.Lock };
            IsSuccessful = lockResult.Lock != null;
            ConflictingLocks = lockResult.ConflictingLocks?.GetLocks().ToList();
            IsTemporaryLock = true;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IActiveLock> OwnedLocks { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<IActiveLock> ConflictingLocks { get; }

        /// <inheritdoc />
        public bool IsTemporaryLock { get; }

        /// <inheritdoc />
        public bool IsSuccessful { get; }

        /// <inheritdoc />
        public IWebDavResult CreateErrorResponse()
        {
            if (IsSuccessful)
                throw new InvalidOperationException("No error to create a response for.");

            if (ConflictingLocks == null)
            {
                // No "If" header condition succeeded, but we didn't ask for a lock
                return new WebDavResult(WebDavStatusCode.NotFound);
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
            if (!IsTemporaryLock)
                return Task.CompletedTask;

            // A temporary lock is always on its own
            var l = OwnedLocks.Single();

            // Ignore errors, because the only error that may happen
            // is, that the lock already expired.
            return _lockManager.ReleaseAsync(l.Path, new Uri(l.StateToken, UriKind.RelativeOrAbsolute), cancellationToken);
        }
    }
}
