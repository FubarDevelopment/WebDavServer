// <copyright file="IDocumentTarget.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Engines
{
    /// <summary>
    /// Interface for a target that's a document
    /// </summary>
    /// <typeparam name="TCollection">The interface type for a collection target</typeparam>
    /// <typeparam name="TDocument">The interface type for a document target</typeparam>
    /// <typeparam name="TMissing">The interface type for a missing target</typeparam>
    public interface IDocumentTarget<TCollection, TDocument, TMissing> : IExistingTarget
        where TCollection : class, ICollectionTarget<TCollection, TDocument, TMissing>
        where TDocument : class, IDocumentTarget<TCollection, TDocument, TMissing>
        where TMissing : class, IMissingTarget<TCollection, TDocument, TMissing>
    {
        /// <summary>
        /// Delete the document target.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The document that's now a missing target (because it was deleted by this function).</returns>
        Task<TMissing> DeleteAsync(CancellationToken cancellationToken);
    }
}
