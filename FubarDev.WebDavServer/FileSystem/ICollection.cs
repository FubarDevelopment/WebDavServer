// <copyright file="ICollection.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface ICollection : IEntry
    {
        [NotNull]
        [ItemCanBeNull]
        Task<IEntry> GetChildAsync([NotNull] string name, CancellationToken ct);

        [NotNull]
        [ItemNotNull]
        Task<IReadOnlyCollection<IEntry>> GetChildrenAsync(CancellationToken ct);

        [NotNull]
        [ItemNotNull]
        Task<IDocument> CreateDocumentAsync([NotNull] string name, CancellationToken ct);

        [NotNull]
        [ItemNotNull]
        Task<ICollection> CreateCollectionAsync([NotNull] string name, CancellationToken ct);
    }
}
