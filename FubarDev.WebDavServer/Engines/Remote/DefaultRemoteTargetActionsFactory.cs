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
    public class DefaultRemoteTargetActionsFactory : IRemoteCopyTargetActionsFactory, IRemoteMoveTargetActionsFactory
    {
        private readonly IHttpMessageHandlerFactory _httpMessageHandlerFactory;

        public DefaultRemoteTargetActionsFactory(IHttpMessageHandlerFactory httpMessageHandlerFactory)
        {
            _httpMessageHandlerFactory = httpMessageHandlerFactory;
        }

        public async Task<IRemoteCopyTargetActions> CreateCopyTargetActionsAsync(Uri destinationUrl, CancellationToken cancellationToken)
        {
            // Copy or move from server to server (slow)
            if (_httpMessageHandlerFactory == null)
                throw new WebDavException(WebDavStatusCode.BadGateway, "No HttpClient factory for remote access");

            var parentCollectionUrl = destinationUrl.GetParent();
            var httpMessageHandler = await _httpMessageHandlerFactory.CreateAsync(parentCollectionUrl, cancellationToken).ConfigureAwait(false);
            if (httpMessageHandler == null)
                throw new WebDavException(WebDavStatusCode.BadGateway, "No HttpClient created");

            var httpClient = new HttpClient(httpMessageHandler)
            {
                BaseAddress = destinationUrl,
            };

            return new CopyRemoteHttpClientTargetActions(httpClient);
        }

        public async Task<IRemoteMoveTargetActions> CreateMoveTargetActionsAsync(Uri destinationUrl, CancellationToken cancellationToken)
        {
            // Copy or move from server to server (slow)
            if (_httpMessageHandlerFactory == null)
                throw new WebDavException(WebDavStatusCode.BadGateway, "No HttpClient factory for remote access");

            var parentCollectionUrl = destinationUrl.GetParent();
            var httpMessageHandler = await _httpMessageHandlerFactory.CreateAsync(parentCollectionUrl, cancellationToken).ConfigureAwait(false);
            if (httpMessageHandler == null)
                throw new WebDavException(WebDavStatusCode.BadGateway, "No HttpClient created");

            var httpClient = new HttpClient(httpMessageHandler)
            {
                BaseAddress = destinationUrl,
            };

            return new MoveRemoteHttpClientTargetActions(httpClient);
        }
    }
}
