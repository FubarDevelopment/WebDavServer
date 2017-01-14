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

        private readonly IGetHandler _getHandler;

        private readonly IHeadHandler _headHandler;

        private readonly IOptionsHandler _optionsHandler;

        public WebDavDispatcherClass1(IOptionsHandler optionsHandler, IGetHandler getHandler, IHeadHandler headHandler, IPropFindHandler propFindHandler)
        {
            _propFindHandler = propFindHandler;
            _getHandler = getHandler;
            _headHandler = headHandler;
            _optionsHandler = optionsHandler;

            HttpMethods = new IHandler[] { optionsHandler, getHandler, headHandler, propFindHandler }
                .Where(x => x != null)
                .SelectMany(x => x.HttpMethods)
                .Distinct().ToList();
        }

        public int Version { get; } = 1;

        public IEnumerable<string> HttpMethods { get; }

        public Task<IWebDavResult> GetAsync(string path, CancellationToken cancellationToken)
        {
            return _getHandler.HandleAsync(path, cancellationToken);
        }

        public Task<IWebDavResult> HeadAsync(string path, CancellationToken cancellationToken)
        {
            return _headHandler.HandleAsync(path, cancellationToken);
        }

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
