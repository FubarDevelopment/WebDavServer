using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IDeleteHandler : IClass1Handler
    {
        Task<IWebDavResult> DeleteAsync(string path, CancellationToken cancellationToken);
    }
}
