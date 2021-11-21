// <copyright file="DefaultRemoteTargetActionsFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Engines.Remote
{
    /// <summary>
    /// The factory class for remote target actions.
    /// </summary>
    public class DefaultRemoteTargetActionsFactory : IRemoteCopyTargetActionsFactory, IRemoteMoveTargetActionsFactory
    {
        private readonly IWebDavContextAccessor _contextAccessor;

        private readonly IHttpMessageHandlerFactory _httpMessageHandlerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRemoteTargetActionsFactory"/> class.
        /// </summary>
        /// <param name="contextAccessor">The WebDAV request context accessor.</param>
        /// <param name="httpMessageHandlerFactory">The factory for <see cref="HttpClient"/> instances</param>
        public DefaultRemoteTargetActionsFactory(
            IWebDavContextAccessor contextAccessor,
            IHttpMessageHandlerFactory httpMessageHandlerFactory)
        {
            _contextAccessor = contextAccessor;
            _httpMessageHandlerFactory = httpMessageHandlerFactory;
        }

        /// <inheritdoc />
        public async Task<IRemoteCopyTargetActions?> CreateCopyTargetActionsAsync(Uri destinationUrl, CancellationToken cancellationToken)
        {
            // Copy or move from server to server (slow)
            if (_httpMessageHandlerFactory == null)
            {
                throw new WebDavException(WebDavStatusCode.BadGateway, "No HttpClient factory for remote access");
            }

            var parentCollectionUrl = destinationUrl.GetParent();
            var httpMessageHandler = await _httpMessageHandlerFactory.CreateAsync(parentCollectionUrl, cancellationToken).ConfigureAwait(false);
            if (httpMessageHandler == null)
            {
                throw new WebDavException(WebDavStatusCode.BadGateway, "No HttpClient created");
            }

            var httpClient = new HttpClient(httpMessageHandler)
            {
                BaseAddress = destinationUrl,
            };

            return new CopyRemoteHttpClientTargetActions(_contextAccessor.WebDavContext, httpClient);
        }

        /// <inheritdoc />
        public async Task<IRemoteMoveTargetActions?> CreateMoveTargetActionsAsync(Uri destinationUrl, CancellationToken cancellationToken)
        {
            // Copy or move from server to server (slow)
            if (_httpMessageHandlerFactory == null)
            {
                throw new WebDavException(WebDavStatusCode.BadGateway, "No HttpClient factory for remote access");
            }

            var parentCollectionUrl = destinationUrl.GetParent();
            var httpMessageHandler = await _httpMessageHandlerFactory.CreateAsync(parentCollectionUrl, cancellationToken).ConfigureAwait(false);
            if (httpMessageHandler == null)
                throw new WebDavException(WebDavStatusCode.BadGateway, "No HttpClient created");

            var httpClient = new HttpClient(httpMessageHandler)
            {
                BaseAddress = destinationUrl,
            };

            return new MoveRemoteHttpClientTargetActions(_contextAccessor.WebDavContext, httpClient);
        }
    }
}
