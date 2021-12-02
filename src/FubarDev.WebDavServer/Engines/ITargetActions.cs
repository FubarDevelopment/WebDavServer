// <copyright file="ITargetActions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Engines
{
    /// <summary>
    /// The interface for target actions
    /// </summary>
    /// <typeparam name="TCollection">The interface type for a collection target</typeparam>
    /// <typeparam name="TDocument">The interface type for a document target</typeparam>
    /// <typeparam name="TMissing">The interface type for a missing target</typeparam>
    public interface ITargetActions<in TCollection, TDocument, in TMissing>
        where TCollection : class, ICollectionTarget<TCollection, TDocument, TMissing>
        where TDocument : class, IDocumentTarget<TCollection, TDocument, TMissing>
        where TMissing : class, IMissingTarget<TCollection, TDocument, TMissing>
    {
        /// <summary>
        /// Gets the WebDAV context.
        /// </summary>
        /// <remarks>
        /// This is required to get all the predefined (and live) properties.
        /// </remarks>
        IWebDavContext Context { get; }

        /// <summary>
        /// Gets the behaviour of this implementation when a target already exists.
        /// </summary>
        RecursiveTargetBehaviour ExistingTargetBehaviour { get; }

        /// <summary>
        /// Copies or moves a document to a target that doesn't exist.
        /// </summary>
        /// <param name="source">The source document.</param>
        /// <param name="destination">The target where the document should be copied or moved to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created document target.</returns>
        Task<TDocument> ExecuteAsync(IDocument source, TMissing destination, CancellationToken cancellationToken);

        /// <summary>
        /// Copies or moves a document to an existing target.
        /// </summary>
        /// <remarks>
        /// The <paramref name="source"/> can only be copied to the <paramref name="destination"/> when
        /// overwriting it is allowed.
        /// </remarks>
        /// <param name="source">The source document.</param>
        /// <param name="destination">The destination document to overwrite.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result information about this action.</returns>
        Task<ActionResult> ExecuteAsync(IDocument source, TDocument destination, CancellationToken cancellationToken);

        /// <summary>
        /// Do some cleanup after all child elements of the <paramref name="source"/> are processed (copied or moved).
        /// </summary>
        /// <remarks>
        /// In the case of a move operation, the source must be deleted by this implementation.
        /// </remarks>
        /// <param name="source">The source collection.</param>
        /// <param name="destination">The destination collection.</param>
        /// <param name="childResults">The result of the child operations.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task for async execution.</returns>
        Task CleanupAsync(
            ICollection source,
            TCollection destination,
            IEnumerable<ActionResult> childResults,
            CancellationToken cancellationToken);
    }
}
