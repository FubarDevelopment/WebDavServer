using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface ICollection : IEntry
    {
        Task<IEntry> GetChildAsync(string name, CancellationToken ct);
        Task<IReadOnlyCollection<IEntry>> GetChildrenAsync(CancellationToken ct);
    }
}
