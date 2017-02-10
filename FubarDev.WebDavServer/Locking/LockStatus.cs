// <copyright file="LockStatus.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Locking
{
    public class LockStatus
    {
        public LockStatus(
            [NotNull] [ItemNotNull] IReadOnlyCollection<IActiveLock> referenceLocks,
            [NotNull] [ItemNotNull] IReadOnlyCollection<IActiveLock> parentLocks,
            [NotNull] [ItemNotNull] IReadOnlyCollection<IActiveLock> childLocks)
        {
            ReferenceLocks = referenceLocks;
            ParentLocks = parentLocks;
            ChildLocks = childLocks;
        }

        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<IActiveLock> ReferenceLocks { get; }

        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<IActiveLock> ParentLocks { get; }

        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<IActiveLock> ChildLocks { get; }

        public bool IsEmpty => Count == 0;

        public int Count => ReferenceLocks.Count + ParentLocks.Count + ChildLocks.Count;

        public IEnumerable<IActiveLock> GetLocks()
        {
            return ParentLocks.Concat(ReferenceLocks).Concat(ChildLocks);
        }
    }
}
