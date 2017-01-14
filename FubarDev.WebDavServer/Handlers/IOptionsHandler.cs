using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IOptionsHandler : IHandler
    {
        Task<IWebDavResult> HandleAsync(string path, CancellationToken cancellationToken);
    }
}
