using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines
{
    public interface ITargetActions<in TCollection, TDocument, in TMissing>
        where TCollection : ICollectionTarget<TCollection, TDocument, TMissing>
        where TDocument : IDocumentTarget<TCollection, TDocument, TMissing>
        where TMissing : IMissingTarget<TCollection, TDocument, TMissing>
    {
        RecursiveTargetBehaviour ExistingTargetBehaviour { get; }

        [NotNull, ItemNotNull]
        Task<TDocument> ExecuteAsync([NotNull] IDocument source, [NotNull] TMissing destination, CancellationToken cancellationToken);

        [NotNull]
        Task<ActionResult> ExecuteAsync([NotNull] IDocument source, [NotNull] TDocument destination, CancellationToken cancellationToken);

        [NotNull]
        Task ExecuteAsync([NotNull] ICollection source, CancellationToken cancellationToken);
    }
}
