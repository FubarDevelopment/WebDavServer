﻿// <copyright file="LockStatus.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// A list of locks affecting a single lock (request).
    /// </summary>
    public class LockStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockStatus"/> class.
        /// </summary>
        /// <param name="referenceLocks">The locks found at the reference position.</param>
        /// <param name="parentLocks">The locks found at positions higher in the hierarchy.</param>
        /// <param name="childLocks">The locks found at positions lower in the hierarchy.</param>
        public LockStatus(
            IReadOnlyCollection<IActiveLock> referenceLocks,
            IReadOnlyCollection<IActiveLock> parentLocks,
            IReadOnlyCollection<IActiveLock> childLocks)
        {
            ReferenceLocks = referenceLocks;
            ParentLocks = parentLocks;
            ChildLocks = childLocks;
        }

        /// <summary>
        /// Gets the empty lock status.
        /// </summary>
        public static LockStatus Empty { get; } = new(
            Array.Empty<IActiveLock>(),
            Array.Empty<IActiveLock>(),
            Array.Empty<IActiveLock>());

        /// <summary>
        /// Gets the locks found at the reference position.
        /// </summary>
        public IReadOnlyCollection<IActiveLock> ReferenceLocks { get; }

        /// <summary>
        /// Gets the locks found at positions higher in the hierarchy.
        /// </summary>
        public IReadOnlyCollection<IActiveLock> ParentLocks { get; }

        /// <summary>
        /// Gets the locks found at positions lower in the hierarchy.
        /// </summary>
        public IReadOnlyCollection<IActiveLock> ChildLocks { get; }

        /// <summary>
        /// Gets a value indicating whether there where no locks found at all.
        /// </summary>
        public bool IsEmpty => Count == 0;

        /// <summary>
        /// Gets the number of found locks.
        /// </summary>
        public int Count => ReferenceLocks.Count + ParentLocks.Count + ChildLocks.Count;

        /// <summary>
        /// Gets all found locks.
        /// </summary>
        /// <returns>all found locks.</returns>
        public IEnumerable<IActiveLock> GetLocks()
        {
            return ParentLocks.Concat(ReferenceLocks).Concat(ChildLocks);
        }
    }
}
