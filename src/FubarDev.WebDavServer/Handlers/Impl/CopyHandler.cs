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
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Model.Headers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    /// <summary>
    /// The implementation of the <see cref="ICopyHandler"/> interface.
    /// </summary>
    public class CopyHandler : CopyMoveHandlerBase, ICopyHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CopyHandlerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyHandler"/> class.
        /// </summary>
        /// <param name="rootFileSystem">The root file system.</param>
        /// <param name="contextAccessor">The WebDAV context accessor.</param>
        /// <param name="implicitLockFactory">A factory to create implicit locks.</param>
        /// <param name="options">The options for the <c>COPY</c> handler.</param>
        /// <param name="logger">The logger for this handler.</param>
        /// <param name="serviceProvider">The service provider used to lazily query the <see cref="IRemoteCopyTargetActionsFactory"/> implementation.</param>
        public CopyHandler(
            IFileSystem rootFileSystem,
            IWebDavContextAccessor contextAccessor,
            IImplicitLockFactory implicitLockFactory,
            IOptions<CopyHandlerOptions> options,
            ILogger<CopyHandler> logger,
            IServiceProvider serviceProvider)
            : base(rootFileSystem, contextAccessor, implicitLockFactory, logger)
        {
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "COPY" };

        /// <inheritdoc />
        public Task<IWebDavResult> CopyAsync(string path, Uri destination, CancellationToken cancellationToken)
        {
            var doOverwrite = WebDavContext.RequestHeaders.Overwrite ?? _options.OverwriteAsDefault;
            var depth = WebDavContext.RequestHeaders.Depth ?? DepthHeader.Infinity;
            return ExecuteAsync(path, destination, depth, doOverwrite, _options.Mode, false, cancellationToken);
        }

        /// <inheritdoc />
        protected override async Task<IRemoteTargetActions?> CreateRemoteTargetActionsAsync(Uri destinationUrl, CancellationToken cancellationToken)
        {
            var remoteTargetActionsFactory = _serviceProvider.GetService<IRemoteCopyTargetActionsFactory>();
            if (remoteTargetActionsFactory != null)
            {
                var targetActions = await remoteTargetActionsFactory
                    .CreateCopyTargetActionsAsync(destinationUrl, cancellationToken).ConfigureAwait(false);
                if (targetActions != null)
                {
                    return targetActions;
                }
            }

            return null;
        }

        /// <inheritdoc />
        protected override ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> CreateLocalTargetActions(RecursiveProcessingMode mode)
        {
            if (mode == RecursiveProcessingMode.PreferFastest)
            {
                return new CopyInFileSystemTargetAction(WebDavContext);
            }

            return new CopyBetweenFileSystemsTargetAction(WebDavContext);
        }
    }
}
