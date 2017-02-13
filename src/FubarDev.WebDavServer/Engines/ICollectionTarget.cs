// <copyright file="ICollectionTarget.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines
{
    public interface ICollectionTarget<TCollection, TDocument, TMissing> : IExistingTarget
        where TMissing : class, IMissingTarget<TCollection, TDocument, TMissing>
        where TDocument : class, IDocumentTarget<TCollection, TDocument, TMissing>
        where TCollection : class, ICollectionTarget<TCollection, TDocument, TMissing>
    {
        bool Created { get; }

        [NotNull]
        [ItemNotNull]
        Task<TMissing> DeleteAsync(CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<ITarget> GetAsync([NotNull] string name, CancellationToken cancellationToken);

        [NotNull]
        TMissing NewMissing([NotNull] string name);
    }
}
