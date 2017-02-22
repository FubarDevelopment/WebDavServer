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
    public class ImplicitLock : IImplicitLock
    {
        private readonly ILockManager _lockManager;

        public ImplicitLock(bool isSuccess = false)
        {
            // false = All "If" header conditions failed
            // true = No lock manager, proceed
            IsSuccessful = isSuccess;
        }

        public ImplicitLock([NotNull] [ItemNotNull] IReadOnlyCollection<IActiveLock> ownedLocks)
        {
            OwnedLocks = ownedLocks;
            IsSuccessful = true;
            IsTemporaryLock = false;
        }

        public ImplicitLock([NotNull] ILockManager lockManager, [NotNull] LockResult lockResult)
        {
            _lockManager = lockManager;
            if (lockResult.Lock != null)
                OwnedLocks = new[] { lockResult.Lock };
            IsSuccessful = lockResult.Lock != null;
            ConflictingLocks = lockResult.ConflictingLocks?.GetLocks().ToList();
            IsTemporaryLock = true;
        }

        public IReadOnlyCollection<IActiveLock> OwnedLocks { get; }

        public IReadOnlyCollection<IActiveLock> ConflictingLocks { get; }

        public bool IsTemporaryLock { get; }

        public bool IsSuccessful { get; }

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

        public Task DisposeAsync(CancellationToken cancellationToken)
        {
            if (!IsTemporaryLock)
                return Task.FromResult(0);

            // A temporary lock is always on its own
            var l = OwnedLocks.Single();

            // Ignore errors, because the only error that may happen
            // is, that the lock already expired.
            return _lockManager.ReleaseAsync(l.Path, new Uri(l.StateToken, UriKind.RelativeOrAbsolute), cancellationToken);
        }
    }
}
