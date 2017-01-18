using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface ICollection : IEntry
    {
        [NotNull]
        [ItemCanBeNull]
        Task<IEntry> GetChildAsync([NotNull] string name, CancellationToken ct);

        [NotNull]
        [ItemNotNull]
        Task<IReadOnlyCollection<IEntry>> GetChildrenAsync(CancellationToken ct);

        [NotNull]
        [ItemNotNull]
        Task<IDocument> CreateDocumentAsync([NotNull] string name, CancellationToken ct);

        [NotNull]
        [ItemNotNull]
        Task<ICollection> CreateCollectionAsync([NotNull] string name, CancellationToken ct);

        [NotNull]
        [ItemNotNull]
        Task<CollectionActionResult> CopyToAsync([NotNull] ICollection collection, bool recursive, CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<CollectionActionResult> CopyToAsync([NotNull] ICollection collection, [NotNull] string name, bool recursive, CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<CollectionActionResult> MoveToAsync([NotNull] ICollection collection, bool recursive, CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<CollectionActionResult> MoveToAsync([NotNull] ICollection collection, [NotNull] string name, bool recursive, CancellationToken cancellationToken);
    }
}
