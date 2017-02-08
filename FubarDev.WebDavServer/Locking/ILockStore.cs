// <copyright file="ILockStore.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Locking
{
    public interface ILockStore
    {
        Task<IActiveLock> LockAsync(ILock l);

        Task<IReadOnlyCollection<ActiveLock>> FindAsync(Uri targetUrl);

        Task<LockReleaseStatus> ReleaseAsync(Uri stateToken);

        Task<IQueryable<IActiveLock>> QueryAsync();
    }
}
