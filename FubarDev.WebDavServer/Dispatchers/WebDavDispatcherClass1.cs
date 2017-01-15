using System;
using System.Collections.Generic;
using System.IO;
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

        private readonly IPropPatchHandler _propPatchHandler;

        private readonly IGetHandler _getHandler;

        private readonly IHeadHandler _headHandler;

        private readonly IPutHandler _putHandler;

        private readonly IDeleteHandler _deleteHandler;

        private readonly IOptionsHandler _optionsHandler;

        public WebDavDispatcherClass1(IOptionsHandler optionsHandler, IGetHandler getHandler, IHeadHandler headHandler, IPutHandler putHandler, IDeleteHandler deleteHandler, IPropFindHandler propFindHandler, IPropPatchHandler propPatchHandler)
        {
            _propFindHandler = propFindHandler;
            _propPatchHandler = propPatchHandler;
            _getHandler = getHandler;
            _headHandler = headHandler;
            _putHandler = putHandler;
            _deleteHandler = deleteHandler;
            _optionsHandler = optionsHandler;

            HttpMethods = new IHandler[] { optionsHandler, getHandler, headHandler, _putHandler, _deleteHandler, propFindHandler, _propPatchHandler }
                .Where(x => x != null)
                .SelectMany(x => x.HttpMethods)
                .Distinct().ToList();
        }

        public int Version { get; } = 1;

        public IEnumerable<string> HttpMethods { get; }

        public Task<IWebDavResult> GetAsync(string path, CancellationToken cancellationToken)
        {
            return _getHandler.GetAsync(path, cancellationToken);
        }

        public Task<IWebDavResult> HeadAsync(string path, CancellationToken cancellationToken)
        {
            return _headHandler.HeadAsync(path, cancellationToken);
        }

        public Task<IWebDavResult> PutAsync(string path, Stream data, CancellationToken cancellationToken)
        {
            return _putHandler.PutAsync(path, data, cancellationToken);
        }

        public Task<IWebDavResult> OptionsAsync(string path, CancellationToken cancellationToken)
        {
            return _optionsHandler.OptionsAsync(path, cancellationToken);
        }

        public Task<IWebDavResult> PropFindAsync(string path, Propfind request, Depth depth, CancellationToken cancellationToken)
        {
            return _propFindHandler.PropFindAsync(path, request, depth, cancellationToken);
        }

        public Task<IWebDavResult> PropPatch(string path, Propertyupdate request, CancellationToken cancellationToken)
        {
            return _propPatchHandler.PropPatchAsync(path, request, cancellationToken);
        }

        public Task<IWebDavResult> DeleteAsync(string path, CancellationToken cancellationToken)
        {
            return _deleteHandler.DeleteAsync(path, cancellationToken);
        }
    }
}
