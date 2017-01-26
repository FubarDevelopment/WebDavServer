using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines
{
    public interface ICollectionTarget<TCollection, TDocument, TMissing> : IExistingTarget
        where TMissing : IMissingTarget<TCollection, TDocument, TMissing>
        where TDocument : IDocumentTarget<TCollection, TDocument, TMissing>
        where TCollection : ICollectionTarget<TCollection, TDocument, TMissing>
    {
        bool Created { get; }

        [NotNull, ItemNotNull]
        Task<TMissing> DeleteAsync(CancellationToken cancellationToken);

        [NotNull, ItemCanBeNull]
        Task<ITarget> GetAsync([NotNull] string name, CancellationToken cancellationToken);

        [NotNull]
        TMissing CreateMissing([NotNull] string name);
    }
}
