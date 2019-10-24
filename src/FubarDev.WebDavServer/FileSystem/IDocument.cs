// <copyright file="IDocument.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.FileSystem
{
    /// <summary>
    /// The document of a collection.
    /// </summary>
    public interface IDocument : IEntry
    {
        /// <summary>
        /// Gets the length of the document.
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Opens the document for reading.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The stream used to read the document.</returns>
        Task<Stream> OpenReadAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Overwrites the document.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The stream used to write to the document.</returns>
        Task<Stream> CreateAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Copies the document to a new location within the same file system.
        /// </summary>
        /// <param name="collection">The destination collection.</param>
        /// <param name="name">The new name of the document.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created document.</returns>
        Task<IDocument> CopyToAsync(ICollection collection, string name, CancellationToken cancellationToken);

        /// <summary>
        /// Moves the document to a new location within the same file system.
        /// </summary>
        /// <param name="collection">The destination collection.</param>
        /// <param name="name">The new name of the document.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created document.</returns>
        Task<IDocument> MoveToAsync(ICollection collection, string name, CancellationToken cancellationToken);
    }
}
