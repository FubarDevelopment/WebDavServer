using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IPropPatchHandler : IHandler
    {
        Task<IWebDavResult> PropPatchAsync(string path, Propertyupdate request, CancellationToken cancellationToken);
    }
}
