// <copyright file="WebDavDispatcherClass1.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Models;

namespace FubarDev.WebDavServer.Dispatchers
{
    /// <summary>
    /// The default WebDAV class 1 implementation.
    /// </summary>
    public class WebDavDispatcherClass1 : IWebDavClass1
    {
        private readonly IWebDavContextAccessor _contextAccessor;
        private readonly IPropFindHandler? _propFindHandler;
        private readonly IPropPatchHandler? _propPatchHandler;
        private readonly IMkColHandler? _mkColHandler;
        private readonly IGetHandler? _getHandler;
        private readonly IHeadHandler? _headHandler;
        private readonly IPutHandler? _putHandler;
        private readonly IDeleteHandler? _deleteHandler;
        private readonly IOptionsHandler? _optionsHandler;
        private readonly ICopyHandler? _copyHandler;
        private readonly IMoveHandler? _moveHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavDispatcherClass1"/> class.
        /// </summary>
        /// <param name="class1Handlers">The WebDAV class 1 handlers.</param>
        /// <param name="contextAccessor">The WebDAV context accessor.</param>
        public WebDavDispatcherClass1(
            IEnumerable<IClass1Handler> class1Handlers,
            IWebDavContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
            var httpMethods = new HashSet<string>();

            foreach (var class1Handler in class1Handlers)
            {
                var handlerFound = false;

                if (class1Handler is IOptionsHandler optionsHandler)
                {
                    _optionsHandler = optionsHandler;
                    handlerFound = true;
                }

                if (class1Handler is IPropFindHandler propFindHandler)
                {
                    _propFindHandler = propFindHandler;
                    handlerFound = true;
                }

                if (class1Handler is IGetHandler getHandler)
                {
                    _getHandler = getHandler;
                    handlerFound = true;
                }

                if (class1Handler is IHeadHandler headHandler)
                {
                    _headHandler = headHandler;
                    handlerFound = true;
                }

                if (class1Handler is IPropPatchHandler propPatchHandler)
                {
                    _propPatchHandler = propPatchHandler;
                    handlerFound = true;
                }

                if (class1Handler is IPutHandler putHandler)
                {
                    _putHandler = putHandler;
                    handlerFound = true;
                }

                if (class1Handler is IMkColHandler mkColHandler)
                {
                    _mkColHandler = mkColHandler;
                    handlerFound = true;
                }

                if (class1Handler is IDeleteHandler deleteHandler)
                {
                    _deleteHandler = deleteHandler;
                    handlerFound = true;
                }

                if (class1Handler is ICopyHandler copyHandler)
                {
                    _copyHandler = copyHandler;
                    handlerFound = true;
                }

                if (class1Handler is IMoveHandler moveHandler)
                {
                    _moveHandler = moveHandler;
                    handlerFound = true;
                }

                if (!handlerFound)
                {
                    throw new NotSupportedException();
                }

                foreach (var httpMethod in class1Handler.HttpMethods)
                {
                    httpMethods.Add(httpMethod);
                }
            }

            HttpMethods = httpMethods.ToList();

            OptionsResponseHeaders = new Dictionary<string, IEnumerable<string>>()
            {
                ["Allow"] = HttpMethods,
            };

            DefaultResponseHeaders = new Dictionary<string, IEnumerable<string>>()
            {
                ["DAV"] = new[] { "1" },
            };
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; }

        /// <inheritdoc />
        public IWebDavContext WebDavContext => _contextAccessor.WebDavContext;

        /// <inheritdoc />
        public IReadOnlyDictionary<string, IEnumerable<string>> OptionsResponseHeaders { get; }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, IEnumerable<string>> DefaultResponseHeaders { get; }

        /// <inheritdoc />
        public Task<IWebDavResult> GetAsync(string path, CancellationToken cancellationToken)
        {
            if (_getHandler == null)
            {
                throw new NotSupportedException();
            }

            return _getHandler.GetAsync(path, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> HeadAsync(string path, CancellationToken cancellationToken)
        {
            if (_headHandler == null)
            {
                throw new NotSupportedException();
            }

            return _headHandler.HeadAsync(path, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> PutAsync(string path, Stream data, CancellationToken cancellationToken)
        {
            if (_putHandler == null)
            {
                throw new NotSupportedException();
            }

            return _putHandler.PutAsync(path, data, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> OptionsAsync(string path, CancellationToken cancellationToken)
        {
            if (_optionsHandler == null)
            {
                throw new NotSupportedException();
            }

            return _optionsHandler.OptionsAsync(path, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> PropFindAsync(string path, propfind? request, CancellationToken cancellationToken)
        {
            if (_propFindHandler == null)
            {
                throw new NotSupportedException();
            }

            return _propFindHandler.PropFindAsync(path, request, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> PropPatchAsync(string path, propertyupdate request, CancellationToken cancellationToken)
        {
            if (_propPatchHandler == null)
            {
                throw new NotSupportedException();
            }

            return _propPatchHandler.PropPatchAsync(path, request, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> DeleteAsync(string path, CancellationToken cancellationToken)
        {
            if (_deleteHandler == null)
            {
                throw new NotSupportedException();
            }

            return _deleteHandler.DeleteAsync(path, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> MkColAsync(string path, CancellationToken cancellationToken)
        {
            if (_mkColHandler == null)
            {
                throw new NotSupportedException();
            }

            return _mkColHandler.MkColAsync(path, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> CopyAsync(string path, Uri destination, CancellationToken cancellationToken)
        {
            if (_copyHandler == null)
            {
                throw new NotSupportedException();
            }

            return _copyHandler.CopyAsync(path, destination, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> MoveAsync(string path, Uri destination, CancellationToken cancellationToken)
        {
            if (_moveHandler == null)
            {
                throw new NotSupportedException();
            }

            return _moveHandler.MoveAsync(path, destination, cancellationToken);
        }
    }
}
