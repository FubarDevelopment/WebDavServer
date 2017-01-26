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

using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.DefaultHandlers
{
    public class MoveHandler : IMoveHandler
    {
        private readonly IFileSystem _rootFileSystem;
        private readonly IWebDavHost _host;
        private readonly MoveHandlerOptions _options;

        public MoveHandler(IFileSystem rootFileSystem, IWebDavHost host, IOptions<MoveHandlerOptions> options)
        {
            _rootFileSystem = rootFileSystem;
            _host = host;
            _options = options?.Value ?? new MoveHandlerOptions();
        }

        public IEnumerable<string> HttpMethods { get; } = new[] {"MOVE"};

        public async Task<IWebDavResult> MoveAsync(string sourcePath, Uri destination, bool? overwrite, CancellationToken cancellationToken)
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

            ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> handler;
            var isSameFileSystem = ReferenceEquals(sourceSelectionResult.TargetFileSystem, destinationSelectionResult.TargetFileSystem);
            if (isSameFileSystem && _options.Mode == RecursiveProcessingMode.PreferFastest)
            {
                // Copy one item inside the same file system (fast)
                handler = new MoveInFileSystemTargetAction();
            }
            else
            {
                // Copy one item to another file system (probably slow)
                handler = new MoveBetweenFileSystemsTargetAction();
            }

            var doOverwrite = overwrite ?? _options.OverwriteAsDefault;
            var targetInfo = FileSystemTarget.FromSelectionResult(destinationSelectionResult, destinationUrl, handler);
            var targetResult = await MoveAsync(handler, sourceUrl, sourceSelectionResult, targetInfo, doOverwrite, cancellationToken).ConfigureAwait(false);
            return Evaluate(targetResult);
        }

        private IWebDavResult Evaluate(Engines.CollectionActionResult collectionResult)
        {
            if (collectionResult.Status == ActionStatus.Ignored)
            {
                return new WebDavResult(GetWebDavStatusCode(collectionResult.DocumentActionResults.Select(x => x.Status).Single()));
            }

            var resultsByStatus = collectionResult
                .Flatten()
                .Where(x => x.Status != ActionStatus.ParentFailed)
                .GroupBy(x => x.Status)
                .ToDictionary(x => x.Key, x => x.ToList());
            if (resultsByStatus.Count == 1)
            {
                var resultItem = resultsByStatus.Single();
                var statusCode = GetWebDavStatusCode(resultItem.Key);
                if (resultItem.Value.Count == 1)
                {
                    var response = CreateResponse(resultItem.Key, resultItem.Value);
                    if (response.Error != null)
                    {
                        return new WebDavResult<Error>(statusCode, response.Error);
                    }
                }

                return new WebDavResult(statusCode);
            }

            var result = new Multistatus()
            {
                Response = resultsByStatus.Select(x => CreateResponse(x.Key, x.Value)).ToArray()
            };

            return new WebDavResult<Multistatus>(WebDavStatusCodes.MultiStatus, result);
        }

        private WebDavStatusCodes GetWebDavStatusCode(ActionStatus status)
        {
            switch (status)
            {
                case ActionStatus.Created:
                    return WebDavStatusCodes.Created;
                case ActionStatus.Overwritten:
                    return WebDavStatusCodes.NoContent;
                case ActionStatus.OverwriteFailed:
                case ActionStatus.PropSetFailed:
                case ActionStatus.CleanupFailed:
                    return WebDavStatusCodes.Conflict;
                case ActionStatus.CannotOverwrite:
                    return WebDavStatusCodes.PreconditionFailed;
                case ActionStatus.CreateFailed:
                case ActionStatus.TargetDeleteFailed:
                    return WebDavStatusCodes.Forbidden;
                case ActionStatus.ParentFailed:
                    return WebDavStatusCodes.FailedDependency;
            }

            throw new NotSupportedException();
        }

        private Response CreateResponse(ActionStatus status, IEnumerable<ActionResult> result)
        {
            var hrefs = result.Select(x => x.Href.OriginalString).Distinct().ToList();
            var items = new List<Tuple<ItemsChoiceType2, object>>();
            var response = new Response()
            {
                Href = hrefs.First(),
            };

            items.AddRange(hrefs.Skip(1).Select(x => Tuple.Create(ItemsChoiceType2.Href, (object)x)));

            switch (status)
            {
                case ActionStatus.PropSetFailed:
                    response.Error = new Error()
                    {
                        ItemsElementName = new[] { ItemsChoiceType.PreservedLiveProperties, },
                        Items = new[] { new object(), }
                    };
                    break;
            }

            var statusCode = GetWebDavStatusCode(status);
            items.Add(Tuple.Create(ItemsChoiceType2.Status, (object)$"{_host.RequestProtocol} {(int)statusCode} {statusCode.GetReasonPhrase()}"));

            response.ItemsElementName = items.Select(x => x.Item1).ToArray();
            response.Items = items.Select(x => x.Item2).ToArray();

            return response;
        }

        private async Task<Engines.CollectionActionResult> MoveAsync(
            ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> handler,
            Uri sourceUrl,
            SelectionResult sourceSelectionResult,
            FileSystemTarget targetInfo,
            bool overwrite,
            CancellationToken cancellationToken)
        {
            Debug.Assert(sourceSelectionResult.Collection != null, "sourceSelectionResult.Collection != null");

            var engine = new RecursiveExecutionEngine<CollectionTarget, DocumentTarget, MissingTarget>(
                handler,
                overwrite);

            if (sourceSelectionResult.ResultType == SelectionResultType.FoundDocument)
            {
                ActionResult docResult;
                CollectionTarget collParent;
                if (targetInfo.Collection != null)
                {
                    var target = targetInfo.CreateMissingTarget();
                    collParent = target.Parent;
                    // Cannot overwrite collection with document
                    docResult = new ActionResult(ActionStatus.OverwriteFailed, target);
                }
                else if (targetInfo.Document == null)
                {
                    var target = targetInfo.CreateMissingTarget();
                    collParent = target.Parent;
                    docResult = await engine.ExecuteAsync(
                        sourceUrl,
                        sourceSelectionResult.Document,
                        target,
                        cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var target = targetInfo.CreateDocumentTarget();
                    collParent = target.Parent;
                    docResult = await engine.ExecuteAsync(
                        sourceUrl,
                        sourceSelectionResult.Document,
                        target,
                        cancellationToken).ConfigureAwait(false);
                }

                var engineResult = new Engines.CollectionActionResult(ActionStatus.Ignored, collParent)
                {
                    DocumentActionResults = new[] {docResult}
                };

                return engineResult;
            }

            Engines.CollectionActionResult collResult;
            if (targetInfo.Document != null)
            {
                var target = targetInfo.CreateMissingTarget();
                // Cannot overwrite document with collection
                collResult = new Engines.CollectionActionResult(ActionStatus.OverwriteFailed, target);
            }
            else if (targetInfo.Collection == null)
            {
                var target = targetInfo.CreateMissingTarget();
                collResult = await engine.ExecuteAsync(
                    sourceUrl,
                    sourceSelectionResult.Collection,
                    Depth.Infinity, 
                    target,
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var target = targetInfo.CreateCollectionTarget();
                collResult = await engine.ExecuteAsync(
                    sourceUrl,
                    sourceSelectionResult.Collection,
                    Depth.Infinity,
                    target,
                    cancellationToken).ConfigureAwait(false);
            }

            return collResult;
        }
    }
}
