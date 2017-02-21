// <copyright file="IImplicitLock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Locking
{
    public interface IImplicitLock
    {
        IReadOnlyCollection<IActiveLock> OwnedLocks { get; }

        IReadOnlyCollection<IActiveLock> ConflictingLocks { get; }

        bool IsTemporaryLock { get; }

        bool IsSuccessful { get; }

        Task DisposeAsync(CancellationToken cancellationToken);
    }
}
