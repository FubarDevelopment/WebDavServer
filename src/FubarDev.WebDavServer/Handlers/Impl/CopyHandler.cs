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
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    public class CopyHandler : CopyMoveHandlerBase, ICopyHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CopyHandlerOptions _options;

        public CopyHandler(IFileSystem rootFileSystem, IWebDavContext host, IOptions<CopyHandlerOptions> options, ILogger<CopyHandler> logger, IServiceProvider serviceProvider)
            : base(rootFileSystem, host, logger)
        {
            _serviceProvider = serviceProvider;
            _options = options?.Value ?? new CopyHandlerOptions();
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "COPY" };

        /// <inheritdoc />
        public Task<IWebDavResult> CopyAsync(string sourcePath, Uri destination, CancellationToken cancellationToken)
        {
            var doOverwrite = WebDavContext.RequestHeaders.Overwrite ?? _options.OverwriteAsDefault;
            var depth = WebDavContext.RequestHeaders.Depth ?? Depth.Infinity;
            return ExecuteAsync(sourcePath, destination, depth, doOverwrite, _options.Mode, cancellationToken);
        }

        protected override async Task<IRemoteTargetActions> CreateRemoteTargetActionsAsync(Uri destinationUrl, CancellationToken cancellationToken)
        {
            var remoteTargetActionsFactory = _serviceProvider.GetService<IRemoteCopyTargetActionsFactory>();
            if (remoteTargetActionsFactory != null)
            {
                var targetActions = await remoteTargetActionsFactory
                    .CreateCopyTargetActionsAsync(destinationUrl, cancellationToken).ConfigureAwait(false);
                if (targetActions != null)
                    return targetActions;
            }

            return null;
        }

        protected override ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> CreateLocalTargetActions(RecursiveProcessingMode mode)
        {
            if (mode == RecursiveProcessingMode.PreferFastest)
                return new CopyInFileSystemTargetAction();
            return new CopyBetweenFileSystemsTargetAction();
        }
    }
}
