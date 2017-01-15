using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IPutHandler : IHandler
    {
        Task<IWebDavResult> PutAsync(string path, Stream data, CancellationToken cancellationToken);
    }
}
