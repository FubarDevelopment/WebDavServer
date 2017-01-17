using System;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    public interface ICopyHandler : IClass1Handler
    {
        Task<IWebDavResult> CopyAsync(string path, Uri destination, bool forbidOverwrite, CancellationToken cancellationToken);
    }
}
