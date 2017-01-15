using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface ICollection : IEntry
    {
        Task<IEntry> GetChildAsync(string name, CancellationToken ct);
        Task<IReadOnlyCollection<IEntry>> GetChildrenAsync(CancellationToken ct);

        Task<IDocument> CreateDocumentAsync(string name, CancellationToken ct);
        Task<ICollection> CreateCollectionAsync(string name, CancellationToken ct);
    }
}
