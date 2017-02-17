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
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;

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

        [CanBeNull]
        private readonly ILockHandler _lockHandler;

        [CanBeNull]
        private readonly IUnlockHandler _unlockHandler;

        public WebDavDispatcherClass1([NotNull] [ItemNotNull] IEnumerable<IClass1Handler> class1Handlers, [NotNull] IWebDavContext context)
        {
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

                if (class1Handler is ILockHandler lockHandler)
                {
                    _lockHandler = lockHandler;
                    handlerFound = true;
                }

                if (class1Handler is IUnlockHandler unlockHandler)
                {
                    _unlockHandler = unlockHandler;
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
            WebDavContext = context;
        }

        /// <inheritdoc />
        public string Version { get; } = "1";

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; }

        /// <inheritdoc />
        public IWebDavContext WebDavContext { get; }

        /// <inheritdoc />
        public Task<IWebDavResult> GetAsync(string path, CancellationToken cancellationToken)
        {
            if (_getHandler == null)
                throw new NotSupportedException();
            return _getHandler.GetAsync(path, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> HeadAsync(string path, CancellationToken cancellationToken)
        {
            if (_headHandler == null)
                throw new NotSupportedException();
            return _headHandler.HeadAsync(path, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> PutAsync(string path, Stream data, CancellationToken cancellationToken)
        {
            if (_putHandler == null)
                throw new NotSupportedException();
            return _putHandler.PutAsync(path, data, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> OptionsAsync(string path, CancellationToken cancellationToken)
        {
            if (_optionsHandler == null)
                throw new NotSupportedException();
            return _optionsHandler.OptionsAsync(path, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> PropFindAsync(string path, propfind request, CancellationToken cancellationToken)
        {
            if (_propFindHandler == null)
                throw new NotSupportedException();
            return _propFindHandler.PropFindAsync(path, request, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> PropPatchAsync(string path, propertyupdate request, CancellationToken cancellationToken)
        {
            if (_propPatchHandler == null)
                throw new NotSupportedException();
            return _propPatchHandler.PropPatchAsync(path, request, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> DeleteAsync(string path, CancellationToken cancellationToken)
        {
            if (_deleteHandler == null)
                throw new NotSupportedException();
            return _deleteHandler.DeleteAsync(path, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> MkColAsync(string path, CancellationToken cancellationToken)
        {
            if (_mkColHandler == null)
                throw new NotSupportedException();
            return _mkColHandler.MkColAsync(path, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> CopyAsync(string path, Uri destination, CancellationToken cancellationToken)
        {
            if (_copyHandler == null)
                throw new NotSupportedException();
            return _copyHandler.CopyAsync(path, destination, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> MoveAsync(string path, Uri destination, CancellationToken cancellationToken)
        {
            if (_moveHandler == null)
                throw new NotSupportedException();
            return _moveHandler.MoveAsync(path, destination, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> LockAsync(string path, lockinfo info, CancellationToken cancellationToken)
        {
            if (_lockHandler == null)
                throw new NotSupportedException();
            return _lockHandler.LockAsync(path, info, cancellationToken);
        }

        public Task<IWebDavResult> UnlockAsync(string path, LockTokenHeader stateToken, CancellationToken cancellationToken)
        {
            if (_unlockHandler == null)
                throw new NotSupportedException();
            return _unlockHandler.UnlockAsync(path, stateToken, cancellationToken);
        }
    }
}
