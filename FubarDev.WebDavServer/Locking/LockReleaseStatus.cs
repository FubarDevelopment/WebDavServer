using System.Collections.Generic;

namespace FubarDev.WebDavServer.Locking
{
    public class LockReleaseStatus
    {
        public IReadOnlyCollection<ActiveLock> ParentLocks { get; }

        public IReadOnlyCollection<ActiveLock> ChildLocks { get; }

        public ActiveLock ReleaseLock { get; }
    }
}
