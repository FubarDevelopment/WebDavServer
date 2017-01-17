using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IOptionsHandler : IClass1Handler
    {
        Task<IWebDavResult> OptionsAsync(string path, CancellationToken cancellationToken);
    }
}
