using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Engines;
using FubarDev.WebDavServer.Engines.DefaultTargetAction;
using FubarDev.WebDavServer.Engines.FileSystemTargets;
using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.DefaultHandlers
{
    public class CopyHandler : ICopyHandler
    {
        private readonly IFileSystem _rootFileSystem;
        private readonly IWebDavHost _host;
        private readonly CopyHandlerOptions _options;

        public CopyHandler(IFileSystem rootFileSystem, IWebDavHost host, IOptions<CopyHandlerOptions> options)
        {
            _rootFileSystem = rootFileSystem;
            _host = host;
            _options = options?.Value ?? new CopyHandlerOptions();
        }

        public IEnumerable<string> HttpMethods { get; } = new[] {"COPY"};

        public async Task<IWebDavResult> CopyAsync(string sourcePath, Uri destination, Depth depth, bool? overwrite, CancellationToken cancellationToken)
        {
            var sourceSelectionResult = await _rootFileSystem.SelectAsync(sourcePath, cancellationToken).ConfigureAwait(false);
            if (sourceSelectionResult.IsMissing)
                throw new WebDavException(WebDavStatusCodes.NotFound);

            var sourceUrl = new Uri(_host.BaseUrl, sourcePath);
            var destinationUrl = new Uri(sourceUrl, destination);
            if (!_host.BaseUrl.IsBaseOf(destinationUrl) || _options.Mode == RecursiveProcessingMode.PreferCrossServer)
            {
                // Copy from server to server (slow)
                return new WebDavResult(WebDavStatusCodes.BadGateway);
            }

            // Copy from one known file system to another
            var destinationPath = _host.BaseUrl.MakeRelativeUri(destinationUrl).ToString();
            var destinationSelectionResult = await _rootFileSystem.SelectAsync(destinationPath, cancellationToken).ConfigureAwait(false);
            if (destinationSelectionResult.IsMissing && destinationSelectionResult.MissingNames.Count != 1)
                throw new WebDavException(WebDavStatusCodes.Conflict);

            var targetInfo = TargetInfo.FromSelectionResult(destinationSelectionResult, destinationUrl);
            var isSameFileSystem = ReferenceEquals(sourceSelectionResult.TargetFileSystem, destinationSelectionResult.TargetFileSystem);
            if (isSameFileSystem && _options.Mode == RecursiveProcessingMode.PreferFastest)
            {
                // Copy one item inside the same file system (fast)
                return await CopyWithinFileSystemAsync(sourceUrl, sourceSelectionResult, targetInfo, depth, overwrite ?? _options.OverwriteAsDefault, cancellationToken).ConfigureAwait(false);
            }

            // Copy one item to another file system (probably slow)
            return await CopyBetweenFileSystemsAsync(sourceUrl, sourceSelectionResult, targetInfo, depth, overwrite ?? _options.OverwriteAsDefault, cancellationToken).ConfigureAwait(false);
        }

        private async Task<IWebDavResult> CopyBetweenFileSystemsAsync(
            Uri sourceUrl,
            SelectionResult sourceSelectionResult,
            TargetInfo targetInfo,
            Depth depth,
            bool overwrite,
            CancellationToken cancellationToken)
        {
            var handler = new CopyBetweenFileSystemsTargetAction();
            var targetResult = await CopyAsync(handler, sourceUrl, sourceSelectionResult, targetInfo, depth, overwrite, cancellationToken).ConfigureAwait(false);
            return Evaluate(targetResult);
        }

        private async Task<IWebDavResult> CopyWithinFileSystemAsync(
            Uri sourceUrl,
            SelectionResult sourceSelectionResult,
            TargetInfo targetInfo,
            Depth depth,
            bool overwrite,
            CancellationToken cancellationToken)
        {
            var handler = new CopyInFileSystemTargetAction();
            var targetResult = await CopyAsync(handler, sourceUrl, sourceSelectionResult, targetInfo, depth, overwrite, cancellationToken).ConfigureAwait(false);
            return Evaluate(targetResult);
        }

        private IWebDavResult Evaluate(Engines.CollectionActionResult collectionResult)
        {
            if (collectionResult.Status == ActionStatus.Ignored)
            {
                return Evaluate(collectionResult.DocumentActionResults.Select(x => x.Status).Single());
            }

            var resultsByStatus = collectionResult
                .Flatten()
                .Where(x => x.Status != ActionStatus.ParentFailed)
                .GroupBy(x => x.Status)
                .ToDictionary(x => x.Key, x => x.ToList());
            if (resultsByStatus.Count == 1)
                return Evaluate(resultsByStatus.Keys.Single());

            var result = new Multistatus()
            {
                Response = resultsByStatus.Select(x => CreateResponse(x.Key, x.Value)).ToArray()
            };

            return new WebDavResult<Multistatus>(WebDavStatusCodes.MultiStatus, result);
        }

        private IWebDavResult Evaluate(ActionStatus status)
        {
            switch (status)
            {
                case ActionStatus.Created:
                    return new WebDavResult(WebDavStatusCodes.Created);
                case ActionStatus.Overwritten:
                    return new WebDavResult(WebDavStatusCodes.NoContent);
                case ActionStatus.OverwriteFailed:
                case ActionStatus.PropSetFailed:
                case ActionStatus.CleanupFailed:
                    return new WebDavResult(WebDavStatusCodes.Conflict);
                case ActionStatus.CannotOverwrite:
                    return new WebDavResult(WebDavStatusCodes.PreconditionFailed);
                case ActionStatus.CreateFailed:
                case ActionStatus.TargetDeleteFailed:
                    return new WebDavResult(WebDavStatusCodes.Forbidden);
                case ActionStatus.ParentFailed:
                    return new WebDavResult(WebDavStatusCodes.FailedDependency);
            }

            throw new NotSupportedException();
        }

        private Response CreateResponse(ActionStatus status, IEnumerable<Engines.ActionResult> result)
        {
            var hrefs = result.Select(x => x.Href.OriginalString).Distinct().ToList();
            switch (status)
            {
                case ActionStatus.OverwriteFailed:
                case ActionStatus.CleanupFailed:
                    break;
                case ActionStatus.PropSetFailed:
                    break;
            }

            throw new NotImplementedException();
        }

        private async Task<Engines.CollectionActionResult> CopyAsync(
            ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> handler,
            Uri sourceUrl,
            SelectionResult sourceSelectionResult,
            TargetInfo targetInfo,
            Depth depth,
            bool overwrite,
            CancellationToken cancellationToken)
        {
            Debug.Assert(sourceSelectionResult.Collection != null, "sourceSelectionResult.Collection != null");
            Debug.Assert(targetInfo.ParentEntry != null);

            var engine = new RecursiveExecutionEngine<CollectionTarget, DocumentTarget, MissingTarget>(
                handler,
                overwrite);

            if (sourceSelectionResult.ResultType == SelectionResultType.FoundDocument)
            {
                ActionResult docResult;
                CollectionTarget collParent;
                if (targetInfo.TargetEntry == null)
                {
                    collParent = CollectionTarget.Create(
                        targetInfo.DestinationUri,
                        targetInfo.ParentEntry,
                        handler);
                    var docTarget = collParent.CreateMissing(targetInfo.TargetName);
                    docResult = await engine.ExecuteAsync(
                        sourceUrl,
                        sourceSelectionResult.Document,
                        docTarget,
                        cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var doc = targetInfo.TargetEntry as IDocument;
                    if (doc == null)
                    {
                        var docTarget = MissingTarget.Create(
                            targetInfo.DestinationUri,
                            targetInfo.ParentEntry,
                            targetInfo.TargetName, handler);
                        collParent = docTarget.Parent;
                        // Cannot overwrite collection with document
                        docResult = new ActionResult(ActionStatus.OverwriteFailed, docTarget);
                    }
                    else
                    {
                        var docTarget = DocumentTarget.Create(targetInfo.DestinationUri, doc, handler);
                        collParent = docTarget.Parent;
                        docResult = await engine.ExecuteAsync(
                            sourceUrl,
                            sourceSelectionResult.Document,
                            docTarget,
                            cancellationToken).ConfigureAwait(false);
                    }
                }

                var engineResult = new Engines.CollectionActionResult(ActionStatus.Ignored, collParent)
                {
                    DocumentActionResults = new[] {docResult}
                };

                return engineResult;
            }

            Engines.CollectionActionResult collResult;
            if (targetInfo.TargetEntry == null)
            {
                var collParent = CollectionTarget.Create(
                    targetInfo.DestinationUri,
                    targetInfo.ParentEntry,
                    handler);
                var collTarget = collParent.CreateMissing(targetInfo.TargetName);
                collResult = await engine.ExecuteAsync(
                    sourceUrl,
                    sourceSelectionResult.Collection,
                    depth,
                    collTarget,
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var coll = targetInfo.TargetEntry as ICollection;
                if (coll == null)
                {
                    var collTarget = MissingTarget.Create(
                        targetInfo.DestinationUri,
                        targetInfo.ParentEntry,
                        targetInfo.TargetName, handler);
                    // Cannot overwrite document with collection
                    collResult = new Engines.CollectionActionResult(ActionStatus.OverwriteFailed, collTarget);
                }
                else
                {
                    var collTarget = CollectionTarget.Create(
                        targetInfo.DestinationUri,
                        targetInfo.ParentEntry,
                        handler);
                    collResult = await engine.ExecuteAsync(
                        sourceUrl,
                        sourceSelectionResult.Collection,
                        depth,
                        collTarget,
                        cancellationToken).ConfigureAwait(false);
                }
            }

            return collResult;
        }

        private struct TargetInfo
        {
            private TargetInfo([CanBeNull] ICollection parentEntry, [CanBeNull] IEntry targeEntry, [NotNull] string targetName, [NotNull] Uri destinationUri)
            {
                ParentEntry = parentEntry;
                TargetEntry = targeEntry;
                TargetName = targetName;
                DestinationUri = destinationUri;
            }

            [CanBeNull]
            public ICollection ParentEntry { get; }

            [CanBeNull]
            public IEntry TargetEntry { get; }

            [NotNull]
            public string TargetName { get; }

            [NotNull]
            public Uri DestinationUri { get; }

            public static TargetInfo FromSelectionResult(SelectionResult selectionResult, Uri destinationUri)
            {
                if (selectionResult.IsMissing)
                {
                    if (selectionResult.MissingNames.Count != 1)
                        throw new InvalidOperationException();
                    return new TargetInfo(selectionResult.Collection, null, selectionResult.MissingNames.Single(), destinationUri);
                }

                if (selectionResult.ResultType == SelectionResultType.FoundCollection)
                {
                    return new TargetInfo(selectionResult.Collection.Parent, selectionResult.Collection, selectionResult.Collection.Name, destinationUri);
                }

                Debug.Assert(selectionResult.Document != null, "selectionResult.Document != null");
                return new TargetInfo(selectionResult.Collection, selectionResult.Document, selectionResult.Document.Name, destinationUri);
            }
        }
    }
}
