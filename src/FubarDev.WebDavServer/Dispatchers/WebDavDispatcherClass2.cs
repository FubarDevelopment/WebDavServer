// <copyright file="WebDavDispatcherClass2.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Dispatchers
{
    public class WebDavDispatcherClass2 : IWebDavClass2
    {
        [CanBeNull]
        private readonly ILockHandler _lockHandler;

        [CanBeNull]
        private readonly IUnlockHandler _unlockHandler;

        public WebDavDispatcherClass2([NotNull] [ItemNotNull] IEnumerable<IClass2Handler> handlers, [NotNull] IWebDavContext context)
        {
            var httpMethods = new HashSet<string>();

            foreach (var handler in handlers)
            {
                var handlerFound = false;

                if (handler is ILockHandler lockHandler)
                {
                    _lockHandler = lockHandler;
                    handlerFound = true;
                }

                if (handler is IUnlockHandler unlockHandler)
                {
                    _unlockHandler = unlockHandler;
                    handlerFound = true;
                }

                if (!handlerFound)
                {
                    throw new NotSupportedException();
                }

                foreach (var httpMethod in handler.HttpMethods)
                {
                    httpMethods.Add(httpMethod);
                }
            }

            HttpMethods = httpMethods.ToList();
            WebDavContext = context;
        }

        /// <inheritdoc />
        public string Version { get; } = "2";

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; }

        /// <inheritdoc />
        public IWebDavContext WebDavContext { get; }

        /// <inheritdoc />
        public Task<IWebDavResult> LockAsync(string path, lockinfo info, CancellationToken cancellationToken)
        {
            if (_lockHandler == null)
                throw new NotSupportedException();
            return _lockHandler.LockAsync(path, info, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> RefreshLockAsync(string path, IfHeader ifHeader, TimeoutHeader timeoutHeader, CancellationToken cancellationToken)
        {
            if (_lockHandler == null)
                throw new NotSupportedException();
            return _lockHandler.RefreshLockAsync(path, ifHeader, timeoutHeader, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> UnlockAsync(string path, LockTokenHeader stateToken, CancellationToken cancellationToken)
        {
            if (_unlockHandler == null)
                throw new NotSupportedException();
            return _unlockHandler.UnlockAsync(path, stateToken, cancellationToken);
        }
    }
}
