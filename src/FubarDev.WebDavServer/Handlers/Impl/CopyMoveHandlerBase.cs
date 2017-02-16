// <copyright file="CopyMoveHandlerBase.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Engines;
using FubarDev.WebDavServer.Engines.Local;
using FubarDev.WebDavServer.Engines.Remote;
using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    public abstract class CopyMoveHandlerBase
    {
        [NotNull]
        private readonly IFileSystem _rootFileSystem;

        [NotNull]
        private readonly ILogger _logger;

        protected CopyMoveHandlerBase([NotNull] IFileSystem rootFileSystem, [NotNull] IWebDavContext context, [NotNull] ILogger logger)
        {
            _rootFileSystem = rootFileSystem;
            WebDavContext = context;
            _logger = logger;
        }

        [NotNull]
        protected IWebDavContext WebDavContext { get; }

        public async Task<IWebDavResult> ExecuteAsync(
            [NotNull] string sourcePath,
            [NotNull] Uri destination,
            DepthHeader depth,
            bool overwrite,
            RecursiveProcessingMode mode,
            CancellationToken cancellationToken)
        {
            var sourceSelectionResult = await _rootFileSystem.SelectAsync(sourcePath, cancellationToken).ConfigureAwait(false);
            if (sourceSelectionResult.IsMissing)
                throw new WebDavException(WebDavStatusCode.NotFound);

            var baseUrl = WebDavContext.BaseUrl;
            var sourceUrl = new Uri(baseUrl, sourcePath);
            var destinationUrl = new Uri(sourceUrl, destination);

            // Ignore different schemes
            if (!baseUrl.IsBaseOf(destinationUrl) || mode == RecursiveProcessingMode.PreferCrossServer)
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                    _logger.LogTrace("Using cross-server mode");

                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"{baseUrl} is not a base of {destinationUrl}");

                using (var remoteHandler = await CreateRemoteTargetActionsAsync(destinationUrl, cancellationToken).ConfigureAwait(false))
                {
                    if (remoteHandler == null)
                        throw new WebDavException(WebDavStatusCode.BadGateway, "No remote handler for given client");

                    var remoteTargetResult = await RemoteExecuteAsync(remoteHandler, sourceUrl, sourceSelectionResult, destinationUrl, depth, overwrite, cancellationToken).ConfigureAwait(false);
                    return remoteTargetResult.Evaluate(WebDavContext);
                }
            }

            // Copy or move from one known file system to another
            var destinationPath = baseUrl.MakeRelativeUri(destinationUrl).ToString();
            var destinationSelectionResult = await _rootFileSystem.SelectAsync(destinationPath, cancellationToken).ConfigureAwait(false);
            if (destinationSelectionResult.IsMissing && destinationSelectionResult.MissingNames.Count != 1)
            {
                _logger.LogDebug($"{destinationUrl}: The target is missing with the following path parts: {string.Join(", ", destinationSelectionResult.MissingNames)}");
                throw new WebDavException(WebDavStatusCode.Conflict);
            }

            var isSameFileSystem = ReferenceEquals(sourceSelectionResult.TargetFileSystem, destinationSelectionResult.TargetFileSystem);
            var localMode = isSameFileSystem && mode == RecursiveProcessingMode.PreferFastest
                ? RecursiveProcessingMode.PreferFastest
                : RecursiveProcessingMode.PreferCrossFileSystem;
            var handler = CreateLocalTargetActions(localMode);

            var targetInfo = FileSystemTarget.FromSelectionResult(destinationSelectionResult, destinationUrl, handler);
            var targetResult = await LocalExecuteAsync(handler, sourceUrl, sourceSelectionResult, targetInfo, depth, overwrite, cancellationToken).ConfigureAwait(false);
            return targetResult.Evaluate(WebDavContext);
        }

        protected static async Task<Engines.CollectionActionResult> ExecuteAsync<TCollection, TDocument, TMissing>(
            [NotNull] RecursiveExecutionEngine<TCollection, TDocument, TMissing> engine,
            [NotNull] Uri sourceUrl,
            [NotNull] SelectionResult sourceSelectionResult,
            [NotNull] TCollection parentCollection,
            [NotNull] ITarget targetItem,
            DepthHeader depth,
            CancellationToken cancellationToken)
            where TCollection : class, ICollectionTarget<TCollection, TDocument, TMissing>
            where TDocument : class, IDocumentTarget<TCollection, TDocument, TMissing>
            where TMissing : class, IMissingTarget<TCollection, TDocument, TMissing>
        {
            Debug.Assert(sourceSelectionResult.Collection != null, "sourceSelectionResult.Collection != null");

            if (sourceSelectionResult.ResultType == SelectionResultType.FoundDocument)
            {
                ActionResult docResult;
                if (targetItem is TCollection)
                {
                    // Cannot overwrite collection with document
                    docResult = new ActionResult(ActionStatus.OverwriteFailed, targetItem);
                }
                else if (targetItem is TMissing)
                {
                    var target = (TMissing)targetItem;
                    docResult = await engine.ExecuteAsync(
                        sourceUrl,
                        sourceSelectionResult.Document,
                        target,
                        cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var target = (TDocument)targetItem;
                    docResult = await engine.ExecuteAsync(
                        sourceUrl,
                        sourceSelectionResult.Document,
                        target,
                        cancellationToken).ConfigureAwait(false);
                }

                var engineResult = new Engines.CollectionActionResult(ActionStatus.Ignored, parentCollection)
                {
                    DocumentActionResults = new[] { docResult },
                };

                return engineResult;
            }

            Engines.CollectionActionResult collResult;
            if (targetItem is TDocument)
            {
                // Cannot overwrite document with collection
                collResult = new Engines.CollectionActionResult(ActionStatus.OverwriteFailed, targetItem);
            }
            else if (targetItem is TMissing)
            {
                var target = (TMissing)targetItem;
                collResult = await engine.ExecuteAsync(
                    sourceUrl,
                    sourceSelectionResult.Collection,
                    depth,
                    target,
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var target = (TCollection)targetItem;
                collResult = await engine.ExecuteAsync(
                    sourceUrl,
                    sourceSelectionResult.Collection,
                    depth,
                    target,
                    cancellationToken).ConfigureAwait(false);
            }

            return collResult;
        }

        [NotNull]
        [ItemCanBeNull]
        protected abstract Task<IRemoteTargetActions> CreateRemoteTargetActionsAsync(Uri destinationUrl, CancellationToken cancellationToken);

        [NotNull]
        protected abstract ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> CreateLocalTargetActions(RecursiveProcessingMode mode);

        private async Task<Engines.CollectionActionResult> RemoteExecuteAsync(
            [NotNull] IRemoteTargetActions handler,
            [NotNull] Uri sourceUrl,
            [NotNull] SelectionResult sourceSelectionResult,
            [NotNull] Uri targetUrl,
            DepthHeader depth,
            bool overwrite,
            CancellationToken cancellationToken)
        {
            Debug.Assert(sourceSelectionResult.Collection != null, "sourceSelectionResult.Collection != null");

            var parentCollectionUrl = targetUrl.GetParent();

            var engine = new RecursiveExecutionEngine<RemoteCollectionTarget, RemoteDocumentTarget, RemoteMissingTarget>(
                handler,
                overwrite,
                _logger);

            var targetName = targetUrl.GetName();
            var parentName = parentCollectionUrl.GetName();
            var parentCollection = new RemoteCollectionTarget(null, parentName, parentCollectionUrl, false, handler);
            var targetItem = await handler.GetAsync(parentCollection, targetName, cancellationToken).ConfigureAwait(false);

            return await ExecuteAsync(
                    engine,
                    sourceUrl,
                    sourceSelectionResult,
                    parentCollection,
                    targetItem,
                    depth,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task<Engines.CollectionActionResult> LocalExecuteAsync(
            [NotNull] ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> handler,
            [NotNull] Uri sourceUrl,
            [NotNull] SelectionResult sourceSelectionResult,
            [NotNull] FileSystemTarget targetInfo,
            DepthHeader depth,
            bool overwrite,
            CancellationToken cancellationToken)
        {
            Debug.Assert(sourceSelectionResult.Collection != null, "sourceSelectionResult.Collection != null");

            var engine = new RecursiveExecutionEngine<CollectionTarget, DocumentTarget, MissingTarget>(
                handler,
                overwrite,
                _logger);

            CollectionTarget parentCollection;
            ITarget targetItem;
            if (targetInfo.Collection != null)
            {
                var collTarget = targetInfo.NewCollectionTarget();
                parentCollection = collTarget.Parent;
                targetItem = collTarget;
            }
            else if (targetInfo.Document != null)
            {
                var docTarget = targetInfo.NewDocumentTarget();
                parentCollection = docTarget.Parent;
                targetItem = docTarget;
            }
            else
            {
                var missingTarget = targetInfo.NewMissingTarget();
                parentCollection = missingTarget.Parent;
                targetItem = missingTarget;
            }

            Debug.Assert(parentCollection != null, "parentCollection != null");

            return await ExecuteAsync(
                    engine,
                    sourceUrl,
                    sourceSelectionResult,
                    parentCollection,
                    targetItem,
                    depth,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
