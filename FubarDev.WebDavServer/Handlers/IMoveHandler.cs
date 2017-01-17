using System;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IMoveHandler : IClass1Handler
    {
        Task<IWebDavResult> MoveAsync(string path, Uri destination, bool forbidOverwrite, CancellationToken cancellationToken);
    }
}
