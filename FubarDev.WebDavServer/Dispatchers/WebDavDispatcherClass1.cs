using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Dispatchers
{
    public class WebDavDispatcherClass1 : IWebDavClass1
    {
        [CanBeNull]
        private readonly IPropFindHandler _propFindHandler;

        [CanBeNull]
        private readonly IPropPatchHandler _propPatchHandler;

        [CanBeNull]
        private readonly IMkColHandler _mkColHandler;

        [CanBeNull]
        private readonly IGetHandler _getHandler;

        [CanBeNull]
        private readonly IHeadHandler _headHandler;

        [CanBeNull]
        private readonly IPutHandler _putHandler;

        [CanBeNull]
        private readonly IDeleteHandler _deleteHandler;

        [CanBeNull]
        private readonly IOptionsHandler _optionsHandler;

        [CanBeNull]
        private readonly ICopyHandler _copyHandler;

        [CanBeNull]
        private readonly IMoveHandler _moveHandler;

        public WebDavDispatcherClass1(IEnumerable<IClass1Handler> class1Handlers)
        {
            var httpMethods = new HashSet<string>();

            foreach (var class1Handler in class1Handlers)
            {
                if (class1Handler is IOptionsHandler optionsHandler)
                {
                    _optionsHandler = optionsHandler;
                }
                else if (class1Handlers is IPropFindHandler propFindHandler)
                {
                    _propFindHandler = propFindHandler;
                }
                else if (class1Handlers is IGetHandler getHandler)
                {
                    _getHandler = getHandler;
                }
                else if (class1Handlers is IHeadHandler headHandler)
                {
                    _headHandler = headHandler;
                }
                else if (class1Handlers is IPropPatchHandler propPatchHandler)
                {
                    _propPatchHandler = propPatchHandler;
                }
                else if (class1Handlers is IPutHandler putHandler)
                {
                    _putHandler = putHandler;
                }
                else if (class1Handlers is IMkColHandler mkColHandler)
                {
                    _mkColHandler = mkColHandler;
                }
                else if (class1Handlers is IDeleteHandler deleteHandler)
                {
                    _deleteHandler = deleteHandler;
                }
                else if (class1Handlers is ICopyHandler copyHandler)
                {
                    _copyHandler = copyHandler;
                }
                else if (class1Handlers is IMoveHandler moveHandler)
                {
                    _moveHandler = moveHandler;
                }
                else
                {
                    throw new NotSupportedException();
                }

                foreach (var httpMethod in class1Handler.HttpMethods)
                {
                    httpMethods.Add(httpMethod);
                }
            }

            HttpMethods = httpMethods.ToList();
        }

        public int Version { get; } = 1;

        public IEnumerable<string> HttpMethods { get; }

        public Task<IWebDavResult> GetAsync(string path, CancellationToken cancellationToken)
        {
            if (_getHandler == null)
                throw new NotSupportedException();
            return _getHandler.GetAsync(path, cancellationToken);
        }

        public Task<IWebDavResult> HeadAsync(string path, CancellationToken cancellationToken)
        {
            if (_headHandler == null)
                throw new NotSupportedException();
            return _headHandler.HeadAsync(path, cancellationToken);
        }

        public Task<IWebDavResult> PutAsync(string path, Stream data, CancellationToken cancellationToken)
        {
            if (_putHandler == null)
                throw new NotSupportedException();
            return _putHandler.PutAsync(path, data, cancellationToken);
        }

        public Task<IWebDavResult> OptionsAsync(string path, CancellationToken cancellationToken)
        {
            if (_optionsHandler == null)
                throw new NotSupportedException();
            return _optionsHandler.OptionsAsync(path, cancellationToken);
        }

        public Task<IWebDavResult> PropFindAsync(string path, Propfind request, Depth depth, CancellationToken cancellationToken)
        {
            if (_propFindHandler == null)
                throw new NotSupportedException();
            return _propFindHandler.PropFindAsync(path, request, depth, cancellationToken);
        }

        public Task<IWebDavResult> PropPatch(string path, Propertyupdate request, CancellationToken cancellationToken)
        {
            if (_propPatchHandler == null)
                throw new NotSupportedException();
            return _propPatchHandler.PropPatchAsync(path, request, cancellationToken);
        }

        public Task<IWebDavResult> DeleteAsync(string path, CancellationToken cancellationToken)
        {
            if (_deleteHandler == null)
                throw new NotSupportedException();
            return _deleteHandler.DeleteAsync(path, cancellationToken);
        }

        public Task<IWebDavResult> MkColAsync(string path, CancellationToken cancellationToken)
        {
            if (_mkColHandler == null)
                throw new NotSupportedException();
            return _mkColHandler.MkColAsync(path, cancellationToken);
        }

        public Task<IWebDavResult> CopyAsync(string path, Uri destination, bool forbidOverwrite, CancellationToken cancellationToken)
        {
            if (_copyHandler == null)
                throw new NotSupportedException();
            return _copyHandler.CopyAsync(path, destination, forbidOverwrite, cancellationToken);
        }

        public Task<IWebDavResult> MoveAsync(string path, Uri destination, bool forbidOverwrite, CancellationToken cancellationToken)
        {
            if (_moveHandler == null)
                throw new NotSupportedException();
            return _moveHandler.MoveAsync(path, destination, forbidOverwrite, cancellationToken);
        }
    }
}
