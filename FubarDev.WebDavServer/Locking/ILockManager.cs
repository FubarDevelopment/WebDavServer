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
    public interface ILockManager
    {
        Task<Either<IReadOnlyCollection<IActiveLock>, IActiveLock>> LockAsync(ILock l, CancellationToken cancellationToken);

        Task ReleaseAsync(Uri stateToken, CancellationToken cancellationToken);

        Task<IEnumerable<IActiveLock>> GetLocksAsync(CancellationToken cancellationToken);
    }
}
