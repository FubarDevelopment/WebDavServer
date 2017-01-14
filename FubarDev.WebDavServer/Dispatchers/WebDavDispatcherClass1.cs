using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Dispatchers
{
    public class WebDavDispatcherClass1 : IWebDavClass1
    {
        private readonly IPropFindHandler _propFindHandler;

        private readonly IOptionsHandler _optionsHandler;

        public WebDavDispatcherClass1(IPropFindHandler propFindHandler, IOptionsHandler optionsHandler)
        {
            _propFindHandler = propFindHandler;
            _optionsHandler = optionsHandler;

            HttpMethods = new IHandler[] { propFindHandler, optionsHandler }
                .Where(x => x != null)
                .SelectMany(x => x.HttpMethods)
                .Distinct().ToList();
        }

        public int Version { get; } = 1;

        public IEnumerable<string> HttpMethods { get; }

        public Task<IWebDavResult> OptionsAsync(string path, CancellationToken cancellationToken)
        {
            return _optionsHandler.HandleAsync(path, cancellationToken);
        }

        public Task<IWebDavResult> PropFindAsync(string path, Propfind request, Depth depth, CancellationToken cancellationToken)
        {
            return _propFindHandler.HandleAsync(path, request, depth, cancellationToken);
        }
    }
}
