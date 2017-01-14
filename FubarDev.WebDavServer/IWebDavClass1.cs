using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer
{
    public interface IWebDavClass1 : IWebDavClass
    {
        Task<IWebDavResult> OptionsAsync(string path, CancellationToken cancellationToken);

        Task<IWebDavResult> GetAsync(string path, CancellationToken cancellationToken);

        Task<IWebDavResult> HeadAsync(string path, CancellationToken cancellationToken);

        Task<IWebDavResult> PropFindAsync(string path, Propfind request, Depth depth, CancellationToken cancellationToken);
    }
}
