// <copyright file="MoveInFileSystemTargetAction.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Engines.Local
{
    /// <summary>
    /// The <see cref="ITargetActions{TCollection,TDocument,TMissing}"/> implementation that moves only entries within the same file system
    /// </summary>
    public class MoveInFileSystemTargetAction : ITargetActions<CollectionTarget, DocumentTarget, MissingTarget>
    {
        [NotNull]
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveInFileSystemTargetAction"/> class.
        /// </summary>
        /// <param name="dispatcher">The WebDAV dispatcher</param>
        /// <param name="logger">The logger</param>
        public MoveInFileSystemTargetAction([NotNull] IWebDavDispatcher dispatcher, [NotNull] ILogger logger)
        {
            Dispatcher = dispatcher;
            _logger = logger;
        }

        /// <inheritdoc />
        [NotNull]
        public IWebDavDispatcher Dispatcher { get; }

        /// <inheritdoc />
        public RecursiveTargetBehaviour ExistingTargetBehaviour { get; } = RecursiveTargetBehaviour.Overwrite;

        /// <inheritdoc />
        public async Task<DocumentTarget> ExecuteAsync(IDocument source, MissingTarget destination, CancellationToken cancellationToken)
        {
            var doc = await source.MoveToAsync(destination.Parent.Collection, destination.Name, cancellationToken).ConfigureAwait(false);
            return new DocumentTarget(destination.Parent, destination.DestinationUrl, doc, this);
        }

        /// <inheritdoc />
        public async Task<ActionResult> ExecuteAsync(IDocument source, DocumentTarget destination, CancellationToken cancellationToken)
        {
            try
            {
                Debug.Assert(destination.Parent != null, "destination.Parent != null");
                if (destination.Parent == null)
                    throw new InvalidOperationException("The destination document must have a parent collection");
                await source.MoveToAsync(destination.Parent.Collection, destination.Name, cancellationToken).ConfigureAwait(false);
                return new ActionResult(ActionStatus.Overwritten, destination);
            }
            catch (Exception ex)
            {
                return new ActionResult(ActionStatus.OverwriteFailed, destination)
                {
                    Exception = ex,
                };
            }
        }

        /// <inheritdoc />
        public async Task ExecuteAsync(ICollection source, CollectionTarget destination, CancellationToken cancellationToken)
        {
            await CopyETagAsync(source, destination.Collection, cancellationToken).ConfigureAwait(false);

            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"Try to delete {source.Path}");

            await source.DeleteAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task CopyETagAsync(IEntry source, IEntry dest, CancellationToken cancellationToken)
        {
            if (dest is IEntityTagEntry)
                return;

            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"Try to copy ETag from {source.Path} to {dest.Path}");

            var sourcePropStore = source.FileSystem.PropertyStore;
            var destPropStore = dest.FileSystem.PropertyStore;
            if (sourcePropStore != null && destPropStore != null)
            {
                var etag = await sourcePropStore.GetETagAsync(source, cancellationToken).ConfigureAwait(false);
                await destPropStore.SetAsync(dest, etag.ToXml(), cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
