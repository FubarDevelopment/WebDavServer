using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines
{
    public interface ITargetActions<in TCollection, in TDocument, in TMissing>
        where TCollection : ICollectionTarget
        where TDocument : IDocumentTarget
        where TMissing : IMissingTarget
    {
        RecursiveTargetBehaviour ExistingTargetBehaviour { get; }

        [NotNull, ItemNotNull]
        Task<IDocument> ExecuteAsync([NotNull] IDocument source, [NotNull] TMissing destination, CancellationToken cancellationToken);

        [NotNull]
        Task ExecuteAsync([NotNull] IDocument source, [NotNull] TDocument destination, CancellationToken cancellationToken);

        [NotNull]
        Task ExecuteAsync([NotNull] TCollection source, CancellationToken cancellationToken);
    }
}
