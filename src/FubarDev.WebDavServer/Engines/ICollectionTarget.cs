// <copyright file="ICollectionTarget.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines
{
    /// <summary>
    /// Interface for a target that's a collection
    /// </summary>
    /// <typeparam name="TCollection">The interface type for a collection target</typeparam>
    /// <typeparam name="TDocument">The interface type for a document target</typeparam>
    /// <typeparam name="TMissing">The interface type for a missing target</typeparam>
    public interface ICollectionTarget<TCollection, TDocument, TMissing> : IExistingTarget
        where TCollection : class, ICollectionTarget<TCollection, TDocument, TMissing>
        where TDocument : class, IDocumentTarget<TCollection, TDocument, TMissing>
        where TMissing : class, IMissingTarget<TCollection, TDocument, TMissing>
    {
        /// <summary>
        /// Gets a value indicating whether the collection was created by the <see cref="RecursiveExecutionEngine{TCollection,TDocument,TMissing}"/>
        /// </summary>
        bool Created { get; }

        /// <summary>
        /// Delete the collection target
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The collection that's now a missing target (because it was deleted by this function)</returns>
        [NotNull]
        [ItemNotNull]
        Task<TMissing> DeleteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets a child target
        /// </summary>
        /// <param name="name">The name of the child element</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The target which might be missing or a collection or document</returns>
        [NotNull]
        [ItemNotNull]
        Task<ITarget> GetAsync([NotNull] string name, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a missing child element with the given <paramref name="name"/>
        /// </summary>
        /// <param name="name">The name of the new child element</param>
        /// <returns>The missing target</returns>
        [NotNull]
        TMissing NewMissing([NotNull] string name);
    }
}
