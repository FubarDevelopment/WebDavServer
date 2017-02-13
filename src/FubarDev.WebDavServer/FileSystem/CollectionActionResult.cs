// <copyright file="CollectionActionResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem
{
    public class CollectionActionResult
    {
        public CollectionActionResult([NotNull] ICollection target, [NotNull] [ItemNotNull] IReadOnlyCollection<IEntry> createdChildEntries)
            : this(target, createdChildEntries, null, WebDavStatusCode.OK)
        {
            Target = target;
        }

        public CollectionActionResult(
            [NotNull] ICollection target,
            [NotNull] [ItemNotNull] IReadOnlyCollection<IEntry> createdChildEntries,
            [CanBeNull] IEntry failedEntry,
            WebDavStatusCode errorStatusCode)
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

        public WebDavStatusCode ErrorStatusCode { get; }
    }
}
