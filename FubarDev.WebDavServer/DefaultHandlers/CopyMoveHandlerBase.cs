using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Engines;
using FubarDev.WebDavServer.Engines.Local;
using FubarDev.WebDavServer.Engines.Remote;
using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.DefaultHandlers
{
    public abstract class CopyMoveHandlerBase
    {
        [NotNull]
        private readonly IFileSystem _rootFileSystem;

        [NotNull]
        private readonly IWebDavHost _host;

        [CanBeNull]
        private readonly IRemoteHttpClientFactory _remoteHttpClientFactory;

        protected CopyMoveHandlerBase([NotNull] IFileSystem rootFileSystem, [NotNull] IWebDavHost host, [CanBeNull] IRemoteHttpClientFactory remoteHttpClientFactory = null)
        {
            _rootFileSystem = rootFileSystem;
            _host = host;
            _remoteHttpClientFactory = remoteHttpClientFactory;
        }

        public async Task<IWebDavResult> ExecuteAsync(
            [NotNull] string sourcePath,
            [NotNull] Uri destination, 
            Depth depth, 
            bool overwrite, 
            RecursiveProcessingMode mode, 
            CancellationToken cancellationToken)
        {
            var sourceSelectionResult = await _rootFileSystem.SelectAsync(sourcePath, cancellationToken).ConfigureAwait(false);
            if (sourceSelectionResult.IsMissing)
                throw new WebDavException(WebDavStatusCode.NotFound);

            var sourceUrl = new Uri(_host.BaseUrl, sourcePath);
            var destinationUrl = new Uri(sourceUrl, destination);
            if (!_host.BaseUrl.IsBaseOf(destinationUrl) || mode == RecursiveProcessingMode.PreferCrossServer)
            {
                // Copy or move from server to server (slow)
                if (_remoteHttpClientFactory == null)
                    throw new WebDavException(WebDavStatusCode.BadGateway, "No HttpClient factory for remote access");

                var parentCollectionUrl = destinationUrl.GetParent();
                var httpClient = await _remoteHttpClientFactory.CreateAsync(parentCollectionUrl, cancellationToken).ConfigureAwait(false);
                if (httpClient == null)
                    throw new WebDavException(WebDavStatusCode.BadGateway, "No HttpClient created");

                using (var remoteHandler = CreateRemoteTargetActions(httpClient))
                {
                    if (remoteHandler == null)
                        throw new WebDavException(WebDavStatusCode.BadGateway, "No remote handler for given client");

                    var remoteTargetResult = await RemoteExecuteAsync(remoteHandler, sourceUrl, sourceSelectionResult, destinationUrl, depth, overwrite, cancellationToken).ConfigureAwait(false);
                    return remoteTargetResult.Evaluate(_host);
                }
            }

            // Copy or move from one known file system to another
            var destinationPath = _host.BaseUrl.MakeRelativeUri(destinationUrl).ToString();
            var destinationSelectionResult = await _rootFileSystem.SelectAsync(destinationPath, cancellationToken).ConfigureAwait(false);
            if (destinationSelectionResult.IsMissing && destinationSelectionResult.MissingNames.Count != 1)
                throw new WebDavException(WebDavStatusCode.Conflict);

            var isSameFileSystem = ReferenceEquals(sourceSelectionResult.TargetFileSystem, destinationSelectionResult.TargetFileSystem);
            var localMode = isSameFileSystem && mode == RecursiveProcessingMode.PreferFastest
                ? RecursiveProcessingMode.PreferFastest
                : RecursiveProcessingMode.PreferCrossFileSystem;
            var handler = CreateLocalTargetActions(localMode);

            var targetInfo = FileSystemTarget.FromSelectionResult(destinationSelectionResult, destinationUrl, handler);
            var targetResult = await LocalExecuteAsync(handler, sourceUrl, sourceSelectionResult, targetInfo, depth, overwrite, cancellationToken).ConfigureAwait(false);
            return targetResult.Evaluate(_host);
        }

        private async Task<Engines.CollectionActionResult> RemoteExecuteAsync(
            [NotNull] RemoteTargetActions handler,
            [NotNull] Uri sourceUrl,
            [NotNull] SelectionResult sourceSelectionResult,
            [NotNull] Uri targetUrl,
            Depth depth,
            bool overwrite,
            CancellationToken cancellationToken)
        {
            Debug.Assert(sourceSelectionResult.Collection != null, "sourceSelectionResult.Collection != null");

            var parentCollectionUrl = targetUrl.GetParent();

            var engine = new RecursiveExecutionEngine<RemoteCollectionTarget, RemoteDocumentTarget, RemoteMissingTarget>(
                handler,
                overwrite);

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
            Depth depth,
            bool overwrite,
            CancellationToken cancellationToken)
        {
            Debug.Assert(sourceSelectionResult.Collection != null, "sourceSelectionResult.Collection != null");

            var engine = new RecursiveExecutionEngine<CollectionTarget, DocumentTarget, MissingTarget>(
                handler,
                overwrite);

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

        [CanBeNull]
        protected abstract RemoteHttpClientTargetActions CreateRemoteTargetActions([NotNull] HttpClient httpClient);

        [NotNull]
        protected abstract ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> CreateLocalTargetActions(RecursiveProcessingMode mode);

        protected static async Task<Engines.CollectionActionResult> ExecuteAsync<TCollection, TDocument, TMissing>(
            [NotNull] RecursiveExecutionEngine<TCollection, TDocument, TMissing> engine,
            [NotNull] Uri sourceUrl,
            [NotNull] SelectionResult sourceSelectionResult,
            [NotNull] TCollection parentCollection,
            [NotNull] ITarget targetItem,
            Depth depth,
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
                    DocumentActionResults = new[] { docResult }
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

    }
}
