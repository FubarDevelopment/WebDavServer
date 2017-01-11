using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IPropFindHandler
    {
        Depth Depth { get; set; }

        Task<IWebDavResult> HandleAsync(string path, Propfind request, CancellationToken cancellationToken);
    }
}
