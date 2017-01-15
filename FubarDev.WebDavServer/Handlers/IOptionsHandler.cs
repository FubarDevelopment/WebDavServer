using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IOptionsHandler : IHandler
    {
        Task<IWebDavResult> OptionsAsync(string path, CancellationToken cancellationToken);
    }
}
