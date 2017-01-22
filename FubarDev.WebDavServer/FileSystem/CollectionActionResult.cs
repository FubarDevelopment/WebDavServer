using System.Collections.Generic;

using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem
{
    public class CollectionActionResult
    {
        public CollectionActionResult([NotNull] ICollection target, [NotNull, ItemNotNull] IReadOnlyCollection<IEntry> createdChildEntries)
            : this(target, createdChildEntries, null, WebDavStatusCodes.OK)
        {
            Target = target;
        }

        public CollectionActionResult([NotNull] ICollection target, [NotNull, ItemNotNull] IReadOnlyCollection<IEntry> createdChildEntries, [CanBeNull] IEntry failedEntry, WebDavStatusCodes errorStatusCode)
        {
            Target = target;
            CreatedChildEntries = createdChildEntries;
            FailedEntry = failedEntry;
            ErrorStatusCode = errorStatusCode;
        }

        [NotNull]
        public ICollection Target { get; }

        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<IEntry> CreatedChildEntries { get; }

        [CanBeNull]
        public IEntry FailedEntry { get; }

        public WebDavStatusCodes ErrorStatusCode { get; }
    }
}
