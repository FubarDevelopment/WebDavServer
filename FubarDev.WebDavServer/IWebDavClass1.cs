using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer
{
    public interface IWebDavClass1
    {
        Task<IWebDavResult> PropFindAsync(string path, Propfind request, Depth depth, CancellationToken cancellationToken);
    }
}
