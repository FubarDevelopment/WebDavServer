// <copyright file="ImplicitLock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Locking
{
    public class ImplicitLock
    {
        private readonly ILockManager _lockManager;

        public ImplicitLock(IActiveLock activeLock)
        {
            Lock = activeLock;
            IsTemporaryLock = false;
        }

        public ImplicitLock(ILockManager lockManager, LockResult lockResult)
        {
            _lockManager = lockManager;
            Lock = lockResult.Lock;
            ConflictingLocks = lockResult.ConflictingLocks?.GetLocks().ToList();
            IsTemporaryLock = true;
        }

        public IActiveLock Lock { get; }

        public IReadOnlyCollection<IActiveLock> ConflictingLocks { get; }

        public bool IsTemporaryLock { get; }

        public bool IsSuccessful => Lock != null;

        public static async Task<ImplicitLock> CreateAsync(ILockManager lockManager, IWebDavRequestHeaders headers, ILock lockRequirements, CancellationToken cancellationToken)
        {
            var result = await lockManager.LockAsync(lockRequirements, cancellationToken).ConfigureAwait(false);
            return new ImplicitLock(lockManager, result);
        }

        public Task DisposeAsync(CancellationToken cancellationToken)
        {
            if (!IsTemporaryLock)
                return Task.FromResult(0);

            // Ignore errors, because the only error that may happen
            // is, that the lock already expired.
            return _lockManager.ReleaseAsync(Lock.Path, new Uri(Lock.StateToken, UriKind.RelativeOrAbsolute), cancellationToken);
        }
    }
}
