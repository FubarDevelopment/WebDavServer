using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IGetHandler : IClass1Handler
    {
        Task<IWebDavResult> GetAsync(string path, CancellationToken cancellationToken);
    }
}
