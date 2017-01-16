using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IMkColHandler : IHandler
    {
        Task<IWebDavResult> MkColAsync(string path, CancellationToken cancellationToken);
    }
}
