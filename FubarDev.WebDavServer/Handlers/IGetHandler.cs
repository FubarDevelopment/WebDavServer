using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IGetHandler : IHandler
    {
        Task<IWebDavResult> HandleAsync(string path, CancellationToken cancellationToken);
    }
}
