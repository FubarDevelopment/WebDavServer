using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines
{
    public interface IMissingTarget<TCollection, TDocument, TMissing> : ITarget
        where TCollection : class, ICollectionTarget<TCollection, TDocument, TMissing>
        where TDocument : class, IDocumentTarget<TCollection, TDocument, TMissing>
        where TMissing : class, IMissingTarget<TCollection, TDocument, TMissing>
    {
        [NotNull]
        Task<TCollection> CreateCollectionAsync(CancellationToken cancellationToken);
    }
}
