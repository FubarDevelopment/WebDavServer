using System.IO;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface IDocument : IEntry
    {
        long Length { get; }

        [NotNull]
        [ItemNotNull]
        Task<Stream> OpenReadAsync(CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<Stream> CreateAsync(CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<IEntry> CopyToAsync([NotNull] ICollection collection, [NotNull] string name, CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<IEntry> MoveToAsync([NotNull] ICollection collection, [NotNull] string name, CancellationToken cancellationToken);
    }
}
