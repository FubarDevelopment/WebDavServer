// <copyright file="ICollection.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.FileSystem
{
    /// <summary>
    /// Represents a WebDAV collection.
    /// </summary>
    public interface ICollection : IEntry
    {
        /// <summary>
        /// Gets the child entry with the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the child entry to get.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The found entry (or <c>null</c>).</returns>
        Task<IEntry?> GetChildAsync(string name, CancellationToken ct);

        /// <summary>
        /// Gets all child entries.
        /// </summary>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The found entries.</returns>
        Task<IReadOnlyCollection<IEntry>> GetChildrenAsync(CancellationToken ct);

        /// <summary>
        /// Creates a document with the given name.
        /// </summary>
        /// <param name="name">The name of the new document.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The created document.</returns>
        Task<IDocument> CreateDocumentAsync(string name, CancellationToken ct);

        /// <summary>
        /// Creates a child collection.
        /// </summary>
        /// <param name="name">The name of the new collection.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The new collection.</returns>
        Task<ICollection> CreateCollectionAsync(string name, CancellationToken ct);
    }
}
