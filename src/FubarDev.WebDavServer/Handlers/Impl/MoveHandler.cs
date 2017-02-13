// <copyright file="MoveHandler.cs" company="Fubar Development Junker">
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
using FubarDev.WebDavServer.Model;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    public class MoveHandler : CopyMoveHandlerBase, IMoveHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MoveHandlerOptions _options;

        public MoveHandler(IFileSystem rootFileSystem, IWebDavContext host, IOptions<MoveHandlerOptions> options, ILogger<MoveHandler> logger, IServiceProvider serviceProvider)
            : base(rootFileSystem, host, logger)
        {
            _serviceProvider = serviceProvider;
            _options = options?.Value ?? new MoveHandlerOptions();
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "MOVE" };

        /// <inheritdoc />
        public Task<IWebDavResult> MoveAsync(string sourcePath, Uri destination, CancellationToken cancellationToken)
        {
            var doOverwrite = WebDavContext.RequestHeaders.Overwrite ?? _options.OverwriteAsDefault;
            return ExecuteAsync(sourcePath, destination, Depth.Infinity, doOverwrite, _options.Mode, cancellationToken);
        }

        protected override async Task<IRemoteTargetActions> CreateRemoteTargetActionsAsync(Uri destinationUrl, CancellationToken cancellationToken)
        {
            var remoteTargetActionsFactory = _serviceProvider.GetService<IRemoteMoveTargetActionsFactory>();
            if (remoteTargetActionsFactory != null)
            {
                var targetActions = await remoteTargetActionsFactory
                    .CreateMoveTargetActionsAsync(destinationUrl, cancellationToken).ConfigureAwait(false);
                if (targetActions != null)
                    return targetActions;
            }

            return null;
        }

        protected override ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> CreateLocalTargetActions(RecursiveProcessingMode mode)
        {
            if (mode == RecursiveProcessingMode.PreferFastest)
                return new MoveInFileSystemTargetAction();
            return new MoveBetweenFileSystemsTargetAction();
        }
    }
}
