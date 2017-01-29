using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines
{
    public interface IDocumentTarget<TCollection, TDocument, TMissing> : IExistingTarget
        where TMissing : class, IMissingTarget<TCollection, TDocument, TMissing>
        where TDocument : class, IDocumentTarget<TCollection, TDocument, TMissing>
        where TCollection : class, ICollectionTarget<TCollection, TDocument, TMissing>
    {
        [NotNull, ItemNotNull]
        Task<TMissing> DeleteAsync(CancellationToken cancellationToken);
    }
}
