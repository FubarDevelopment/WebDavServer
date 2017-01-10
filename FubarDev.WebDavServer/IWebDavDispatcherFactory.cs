using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer
{
    public interface IWebDavDispatcherFactory
    {
        Task<IWebDavDispatcher> CreateDispatcherAsync(IPrincipal principal, CancellationToken ct);
    }
}
