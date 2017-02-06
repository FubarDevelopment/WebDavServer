// <copyright file="CopyHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Engines;
using FubarDev.WebDavServer.Engines.Local;
using FubarDev.WebDavServer.Engines.Remote;
using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.DefaultHandlers
{
    public class CopyHandler : CopyMoveHandlerBase, ICopyHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CopyHandlerOptions _options;

        public CopyHandler(IFileSystem rootFileSystem, IWebDavHost host, IOptions<CopyHandlerOptions> options, ILogger<CopyHandler> logger, IServiceProvider serviceProvider)
            : base(rootFileSystem, host, logger)
        {
            _serviceProvider = serviceProvider;
            _options = options?.Value ?? new CopyHandlerOptions();
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "COPY" };

        /// <inheritdoc />
        public Task<IWebDavResult> CopyAsync(string sourcePath, Uri destination, Depth depth, bool? overwrite, CancellationToken cancellationToken)
        {
            var doOverwrite = overwrite ?? _options.OverwriteAsDefault;
            return ExecuteAsync(sourcePath, destination, depth, doOverwrite, _options.Mode, cancellationToken);
        }

        protected override async Task<IRemoteTargetActions> CreateRemoteTargetActionsAsync(Uri destinationUrl, CancellationToken cancellationToken)
        {
            if (_options.CreateRemoteCopyTargetActionsAsync != null)
            {
                return await _options
                    .CreateRemoteCopyTargetActionsAsync(_serviceProvider, cancellationToken)
                    .ConfigureAwait(false);
            }

            var remoteHttpClientFactory = _serviceProvider.GetService<IRemoteHttpClientFactory>();

            // Copy or move from server to server (slow)
            if (remoteHttpClientFactory == null)
                throw new WebDavException(WebDavStatusCode.BadGateway, "No HttpClient factory for remote access");

            var parentCollectionUrl = destinationUrl.GetParent();
            var httpClient = await remoteHttpClientFactory.CreateAsync(parentCollectionUrl, cancellationToken).ConfigureAwait(false);
            if (httpClient == null)
                throw new WebDavException(WebDavStatusCode.BadGateway, "No HttpClient created");

            return new CopyRemoteHttpClientTargetActions(httpClient);
        }

        protected override ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> CreateLocalTargetActions(RecursiveProcessingMode mode)
        {
            if (mode == RecursiveProcessingMode.PreferFastest)
                return new CopyInFileSystemTargetAction();
            return new CopyBetweenFileSystemsTargetAction();
        }
    }
}
