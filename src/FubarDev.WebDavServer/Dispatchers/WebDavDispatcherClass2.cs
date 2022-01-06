// <copyright file="WebDavDispatcherClass2.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Models;

using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Dispatchers
{
    /// <summary>
    /// The default WebDAV class 2 implementation.
    /// </summary>
    public class WebDavDispatcherClass2 : IWebDavClass2
    {
        private readonly IWebDavContextAccessor _contextAccessor;
        private readonly ILockHandler? _lockHandler;
        private readonly IUnlockHandler? _unlockHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavDispatcherClass2"/> class.
        /// </summary>
        /// <param name="options">Options for the WebDAV server.</param>
        /// <param name="handlers">The WebDAV class 2 handlers.</param>
        /// <param name="contextAccessor">The WebDAV context accessor.</param>
        public WebDavDispatcherClass2(
            IOptions<WebDavServerOptions> options,
            IEnumerable<IClass2Handler> handlers,
            IWebDavContextAccessor contextAccessor)
        {
            var enabled = options.Value.EnableClass2;
            _contextAccessor = contextAccessor;
            var httpMethods = new HashSet<string>();

            if (enabled)
            {
                // Only add supported HTTP methods and handlers if Class 2 support is enabled
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
            }

            HttpMethods = httpMethods.ToList();

            OptionsResponseHeaders = new Dictionary<string, IEnumerable<string>>()
            {
                ["Allow"] = HttpMethods,
            };

            var defaultResponseHeaders = new Dictionary<string, IEnumerable<string>>();
            if (enabled)
            {
                // Only add "DAV" header if Class 2 support is enabled
                defaultResponseHeaders["DAV"] = new[] { "2" };
            }

            DefaultResponseHeaders = defaultResponseHeaders;
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
        public Task<IWebDavResult> LockAsync(string path, lockinfo info, CancellationToken cancellationToken)
        {
            if (_lockHandler == null)
            {
                throw new NotSupportedException();
            }

            return _lockHandler.LockAsync(path, info, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> RefreshLockAsync(string path, IfHeader ifHeader, TimeoutHeader? timeoutHeader, CancellationToken cancellationToken)
        {
            if (_lockHandler == null)
            {
                throw new NotSupportedException();
            }

            return _lockHandler.RefreshLockAsync(path, ifHeader, timeoutHeader, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> UnlockAsync(string path, LockTokenHeader stateToken, CancellationToken cancellationToken)
        {
            if (_unlockHandler == null)
            {
                throw new NotSupportedException();
            }

            return _unlockHandler.UnlockAsync(path, stateToken, cancellationToken);
        }
    }
}
