using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IHeadHandler : IClass1Handler
    {
        Task<IWebDavResult> HeadAsync(string path, CancellationToken cancellationToken);
    }
}
