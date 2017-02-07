// <copyright file="DefaultRemoteTargetActionsFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Engines.Remote
{
    public class DefaultRemoteTargetActionsFactory : IRemoteCopyTargetActionsFactory, IRemoteMoveTargetActionsFactory
    {
        private readonly IRemoteHttpClientFactory _httpClientFactory;

        public DefaultRemoteTargetActionsFactory(IRemoteHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IRemoteCopyTargetActions> CreateCopyTargetActionsAsync(Uri destinationUrl, CancellationToken cancellationToken)
        {
            // Copy or move from server to server (slow)
            if (_httpClientFactory == null)
                throw new WebDavException(WebDavStatusCode.BadGateway, "No HttpClient factory for remote access");

            var parentCollectionUrl = destinationUrl.GetParent();
            var httpClient = await _httpClientFactory.CreateAsync(parentCollectionUrl, cancellationToken).ConfigureAwait(false);
            if (httpClient == null)
                throw new WebDavException(WebDavStatusCode.BadGateway, "No HttpClient created");

            return new CopyRemoteHttpClientTargetActions(httpClient);
        }

        public async Task<IRemoteMoveTargetActions> CreateMoveTargetActionsAsync(Uri destinationUrl, CancellationToken cancellationToken)
        {
            // Copy or move from server to server (slow)
            if (_httpClientFactory == null)
                throw new WebDavException(WebDavStatusCode.BadGateway, "No HttpClient factory for remote access");

            var parentCollectionUrl = destinationUrl.GetParent();
            var httpClient = await _httpClientFactory.CreateAsync(parentCollectionUrl, cancellationToken).ConfigureAwait(false);
            if (httpClient == null)
                throw new WebDavException(WebDavStatusCode.BadGateway, "No HttpClient created");

            return new MoveRemoteHttpClientTargetActions(httpClient);
        }
    }
}
