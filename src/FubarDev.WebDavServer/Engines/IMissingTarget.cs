// <copyright file="IMissingTarget.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines
{
    /// <summary>
    /// Interface for a target that's missing
    /// </summary>
    /// <typeparam name="TCollection">The interface type for a collection target</typeparam>
    /// <typeparam name="TDocument">The interface type for a document target</typeparam>
    /// <typeparam name="TMissing">The interface type for a missing target</typeparam>
    public interface IMissingTarget<TCollection, TDocument, TMissing> : ITarget
        where TCollection : class, ICollectionTarget<TCollection, TDocument, TMissing>
        where TDocument : class, IDocumentTarget<TCollection, TDocument, TMissing>
        where TMissing : class, IMissingTarget<TCollection, TDocument, TMissing>
    {
        /// <summary>
        /// Creates a collection with the same name of this target.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created collection target.</returns>
        [NotNull]
        [ItemNotNull]
        Task<TCollection> CreateCollectionAsync(CancellationToken cancellationToken);
    }
}
