using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface ICollection
    {
        Task<IReadOnlyCollection<IEntry>> GetChildrenAsync(CancellationToken ct);
    }
}