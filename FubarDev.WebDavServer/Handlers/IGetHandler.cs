using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IGetHandler : IHandler
    {
        Task<IWebDavResult> GetAsync(string path, CancellationToken cancellationToken);
    }
}
