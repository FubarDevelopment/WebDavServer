using System.Threading;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IPropFindHandler
    {
        Depth Depth { get; set; }

        IWebDavResult HandleAsync(string path, Propfind request, CancellationToken cancellationToken);
    }
}
