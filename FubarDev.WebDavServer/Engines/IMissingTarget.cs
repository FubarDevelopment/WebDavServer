using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines
{
    public interface IMissingTarget<TCollection, TDocument, TMissing> : ITarget
        where TCollection : ICollectionTarget<TCollection, TDocument, TMissing>
        where TDocument : IDocumentTarget<TCollection, TDocument, TMissing>
        where TMissing : IMissingTarget<TCollection, TDocument, TMissing>
    {
        [NotNull]
        Task<TCollection> CreateCollectionAsync(CancellationToken cancellationToken);
    }
}
