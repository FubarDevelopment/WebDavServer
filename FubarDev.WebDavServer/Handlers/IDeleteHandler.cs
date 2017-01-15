using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IDeleteHandler : IHandler
    {
        Task<IWebDavResult> DeleteAsync(string path, CancellationToken cancellationToken);
    }
}
