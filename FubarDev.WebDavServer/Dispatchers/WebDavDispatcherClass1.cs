using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Dispatchers
{
    public class WebDavDispatcherClass1 : IWebDavClass1
    {
        private readonly IPropFindHandler _propFindHandler;

        public WebDavDispatcherClass1(IPropFindHandler propFindHandler)
        {
            _propFindHandler = propFindHandler;
        }

        public Task<IWebDavResult> PropFindAsync(string path, Propfind request, Depth depth, CancellationToken cancellationToken)
        {
            return _propFindHandler.HandleAsync(path, request, depth, cancellationToken);
        }
    }
}
