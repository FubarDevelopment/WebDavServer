// <copyright file="RecursiveExecutionEngine.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.Props;
using FubarDev.WebDavServer.Props.Dead;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Engines
{
    /// <summary>
    /// The engine that operates recursively on its targets.
    /// </summary>
    /// <typeparam name="TCollection">The interface type for a collection target.</typeparam>
    /// <typeparam name="TDocument">The interface type for a document target.</typeparam>
    /// <typeparam name="TMissing">The interface type for a missing target.</typeparam>
    public class RecursiveExecutionEngine<TCollection, TDocument, TMissing>
        where TCollection : class, ICollectionTarget<TCollection, TDocument, TMissing>
        where TDocument : class, IDocumentTarget<TCollection, TDocument, TMissing>
        where TMissing : class, IMissingTarget<TCollection, TDocument, TMissing>
    {
        private readonly ITargetActions<TCollection, TDocument, TMissing> _handler;

        private readonly bool _allowOverwrite;

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecursiveExecutionEngine{TCollection,TDocument,TMissing}"/> class.
        /// </summary>
        /// <param name="handler">The handler that performs the operation on the targets.</param>
        /// <param name="allowOverwrite">Indicates whether overwriting the destination is allowed.</param>
        /// <param name="logger">The logger.</param>
        public RecursiveExecutionEngine(ITargetActions<TCollection, TDocument, TMissing> handler, bool allowOverwrite, ILogger logger)
        {
            _handler = handler;
            _allowOverwrite = allowOverwrite;
            _logger = logger;
        }

        /// <summary>
        /// Operates on a collection and the given missing target.
        /// </summary>
        /// <param name="sourceUrl">The root-relative source URL.</param>
        /// <param name="source">The source collection.</param>
        /// <param name="depth">The recursion depth.</param>
        /// <param name="target">The target of the operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result information of the current operation.</returns>
        public async Task<CollectionActionResult> ExecuteAsync(
            Uri sourceUrl,
            ICollection source,
            DepthHeader depth,
            TMissing target,
            CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(
                    "Collecting nodes for operation on collection {SourceUrl} with missing target {DestinationUrl}",
                    sourceUrl,
                    target.DestinationUrl);
            }

            var nodes = await source.GetNodeAsync(depth.OrderValue, cancellationToken).ConfigureAwait(false);
            return await ExecuteAsync(sourceUrl, nodes, target, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Operates on a collection and the given target document.
        /// </summary>
        /// <param name="sourceUrl">The root-relative source URL.</param>
        /// <param name="source">The source collection.</param>
        /// <param name="depth">The recursion depth.</param>
        /// <param name="target">The target document.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result information of the current operation.</returns>
        public async Task<CollectionActionResult> ExecuteAsync(Uri sourceUrl, ICollection source, DepthHeader depth, TDocument target, CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(
                    "Collecting nodes for operation on collection {SourceUrl} with existing target {DestinationUrl}",
                    sourceUrl,
                    target.DestinationUrl);
            }

            if (!_allowOverwrite)
            {
                _logger.LogDebug(
                    "{DestinationUrl}: Cannot overwrite because destination exists",
                    target.DestinationUrl);
                return new CollectionActionResult(ActionStatus.CannotOverwrite, target);
            }

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(
                    "Delete {DestinationUrl} before performing operation on collection {SourceUrl}",
                    target.DestinationUrl,
                    sourceUrl);
            }

            TMissing missingTarget;
            try
            {
                missingTarget = await target.DeleteAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(
                    "{DestinationUrl}: Delete failed with exception {ErrorMessage}",
                    target.DestinationUrl,
                    ex.Message);
                return new CollectionActionResult(ActionStatus.TargetDeleteFailed, target)
                {
                    Exception = ex,
                };
            }

            var nodes = await source.GetNodeAsync(depth.OrderValue, cancellationToken).ConfigureAwait(false);
            var result = await ExecuteAsync(sourceUrl, nodes, missingTarget, cancellationToken).ConfigureAwait(false);
            if (result.Status != ActionStatus.Created)
            {
                return result;
            }

            return result with
            {
                Status = ActionStatus.Overwritten,
            };
        }

        /// <summary>
        /// Operates on a collection and the given target collection.
        /// </summary>
        /// <param name="sourceUrl">The root-relative source URL.</param>
        /// <param name="source">The source collection.</param>
        /// <param name="depth">The recursion depth.</param>
        /// <param name="target">The target collection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result information of the current operation.</returns>
        public async Task<CollectionActionResult> ExecuteAsync(Uri sourceUrl, ICollection source, DepthHeader depth, TCollection target, CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(
                    "Collecting nodes for operation on collection {SourceUrl} with existing target {DestinationUrl}",
                    sourceUrl,
                    target.DestinationUrl);
            }

            var nodes = await source.GetNodeAsync(depth.OrderValue, cancellationToken).ConfigureAwait(false);
            return await ExecuteAsync(sourceUrl, nodes, target, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Operates on a documentation and the given missing target.
        /// </summary>
        /// <param name="sourceUrl">The root-relative source URL.</param>
        /// <param name="source">The source documentation.</param>
        /// <param name="target">The target of the operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result information of the current operation.</returns>
        public async Task<ActionResult> ExecuteAsync(Uri sourceUrl, IDocument source, TMissing target, CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(
                    "Perform operation on document {SourceUrl} with missing target {DestinationUrl}",
                    sourceUrl,
                    target.DestinationUrl);
            }

            try
            {
                var properties = await GetWriteableProperties(source, cancellationToken).ConfigureAwait(false);

                var newDoc = await _handler.ExecuteAsync(source, target, cancellationToken).ConfigureAwait(false);

                var failedPropertyNames = await newDoc.SetPropertiesAsync(properties, cancellationToken).ConfigureAwait(false);
                if (failedPropertyNames.Count != 0)
                {
                    _logger.LogDebug(
                        "{DestinationUrl}: Failed setting properties {PropertyNames}",
                        target.DestinationUrl,
                        string.Join(", ", failedPropertyNames.Select(x => x.ToString())));
                    return new ActionResult(ActionStatus.PropSetFailed, target)
                    {
                        FailedProperties = failedPropertyNames,
                    };
                }

                return new ActionResult(ActionStatus.Created, newDoc);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(
                    "{DestinationUrl}: Failed with exception {ErrorMessage}",
                    target.DestinationUrl,
                    ex.Message);
                return new ActionResult(ActionStatus.CreateFailed, target)
                {
                    Exception = ex,
                };
            }
        }

        /// <summary>
        /// Operates on a document and the given collection target.
        /// </summary>
        /// <param name="sourceUrl">The root-relative source URL.</param>
        /// <param name="source">The source document.</param>
        /// <param name="target">The target collection of the operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result information of the current operation.</returns>
        public async Task<ActionResult> ExecuteAsync(Uri sourceUrl, IDocument source, TCollection target, CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(
                    "Try to perform operation on document {SourceUrl} with existing target {DestinationUrl}",
                    sourceUrl,
                    target.DestinationUrl);
            }

            if (!_allowOverwrite)
            {
                _logger.LogDebug(
                    "{DestinationUrl}: Cannot overwrite because destination exists",
                    target.DestinationUrl);
                return new ActionResult(ActionStatus.CannotOverwrite, target);
            }

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(
                    "Delete {DestinationUrl} before performing operation on document {SourceUrl}",
                    target.DestinationUrl,
                    sourceUrl);
            }

            TMissing missingTarget;
            try
            {
                missingTarget = await target.DeleteAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(
                    "{DestinationUrl}: Delete failed with exception {ErrorMessage}",
                    target.DestinationUrl,
                    ex.Message);
                return new ActionResult(ActionStatus.TargetDeleteFailed, target)
                {
                    Exception = ex,
                };
            }

            var result = await ExecuteAsync(sourceUrl, source, missingTarget, cancellationToken).ConfigureAwait(false);
            if (result.Status != ActionStatus.Created)
            {
                return result;
            }

            return result with
            {
                Status = ActionStatus.Overwritten,
            };
        }

        /// <summary>
        /// Operates on a document and the given document target.
        /// </summary>
        /// <param name="sourceUrl">The root-relative source URL.</param>
        /// <param name="source">The source document.</param>
        /// <param name="target">The target document of the operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result information of the current operation.</returns>
        public async Task<ActionResult> ExecuteAsync(Uri sourceUrl, IDocument source, TDocument target, CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(
                    "Try to perform operation on document {SourceUrl} with existing target {DestinationUrl}",
                    sourceUrl,
                    target.DestinationUrl);
            }

            if (!_allowOverwrite)
            {
                _logger.LogDebug(
                    "{DestinationUrl}: Cannot overwrite because destination exists",
                    target.DestinationUrl);
                return new ActionResult(ActionStatus.CannotOverwrite, target);
            }

            if (_handler.ExistingTargetBehaviour == RecursiveTargetBehaviour.DeleteTarget)
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace(
                        "Delete {DestinationUrl} before performing operation on document {SourceUrl}",
                        target.DestinationUrl,
                        sourceUrl);
                }

                TMissing missingTarget;
                try
                {
                    missingTarget = await target.DeleteAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(
                        "{DestinationUrl}: Delete failed with exception {ErrorMessage}",
                        target.DestinationUrl,
                        ex.Message);
                    return new ActionResult(ActionStatus.TargetDeleteFailed, target)
                    {
                        Exception = ex,
                    };
                }

                var result = await ExecuteAsync(sourceUrl, source, missingTarget, cancellationToken).ConfigureAwait(false);
                if (result.Status != ActionStatus.Created)
                {
                    return result;
                }

                return result with
                {
                    Status = ActionStatus.Overwritten,
                };
            }

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(
                    "Perform operation on document {SourceUrl} with existing target {DestinationUrl}",
                    sourceUrl,
                    target.DestinationUrl);
            }

            var properties = await GetWriteableProperties(source, cancellationToken).ConfigureAwait(false);

            var docActionResult = await _handler.ExecuteAsync(source, target, cancellationToken).ConfigureAwait(false);
            if (docActionResult.IsFailure)
            {
                return docActionResult;
            }

            var failedPropertyNames = await target.SetPropertiesAsync(properties, cancellationToken).ConfigureAwait(false);
            if (failedPropertyNames.Count != 0)
            {
                _logger.LogDebug(
                    "{DestinationUrl}: Failed setting properties {PropertyNames}",
                    target.DestinationUrl,
                    string.Join(", ", failedPropertyNames.Select(x => x.ToString())));
                return new ActionResult(ActionStatus.PropSetFailed, target)
                {
                    FailedProperties = failedPropertyNames,
                };
            }

            return new ActionResult(ActionStatus.Overwritten, target);
        }

        private async Task<List<IUntypedWriteableProperty>> GetWriteableProperties(IEntry entry, CancellationToken cancellationToken)
        {
            var deadPropertyFactory = _handler.Context.RequestServices.GetRequiredService<IDeadPropertyFactory>();
            var properties = await entry
                .GetProperties(deadPropertyFactory)
                .OfType<IUntypedWriteableProperty>()
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            return properties;
        }

        private async Task<CollectionActionResult> ExecuteAsync(
            Uri sourceUrl,
            ICollectionNode sourceNode,
            TMissing target,
            CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(
                    "Collect properties for operation on collection {SourceUrl} and create target {DestinationUrl}",
                    sourceUrl,
                    target.DestinationUrl);
            }

            var properties = await GetWriteableProperties(sourceNode.Collection, cancellationToken).ConfigureAwait(false);

            TCollection newColl;
            try
            {
                newColl = await target.CreateCollectionAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(
                    "{DestinationUrl}: Create failed with exception {ErrorMessage}",
                    target.DestinationUrl,
                    ex.Message);
                return new CollectionActionResult(ActionStatus.CreateFailed, target)
                {
                    Exception = ex,
                };
            }

            var result = await ExecuteAsync(sourceUrl, sourceNode, newColl, properties, cancellationToken).ConfigureAwait(false);
            if (result.Status == ActionStatus.Updated)
            {
                result = result with { Status = ActionStatus.Created };
            }

            return result;
        }

        private async Task<CollectionActionResult> ExecuteAsync(
            Uri sourceUrl,
            ICollectionNode sourceNode,
            TCollection target,
            CancellationToken cancellationToken)
        {
            if (_allowOverwrite && _handler.ExistingTargetBehaviour == RecursiveTargetBehaviour.DeleteTarget)
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace(
                        "Delete existing target {DestinationUrl} for operation on collection {SourceUrl}",
                        target.DestinationUrl,
                        sourceUrl);
                }

                // Only delete an existing collection when the client allows an overwrite
                TMissing missing;
                try
                {
                    missing = await target.DeleteAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(
                        "{DestinationUrl}: Delete failed with exception {ErrorMessage}",
                        target.DestinationUrl,
                        ex.Message);
                    return new CollectionActionResult(ActionStatus.TargetDeleteFailed, target)
                    {
                        Exception = ex,
                    };
                }

                var replaceResult = await ExecuteAsync(sourceUrl, sourceNode, missing, cancellationToken).ConfigureAwait(false);
                if (replaceResult.Status == ActionStatus.Created)
                {
                    replaceResult = replaceResult with { Status = ActionStatus.Overwritten };
                }

                return replaceResult;
            }

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(
                    "Collect properties for operation on collection {SourceUrl} with existing target {DestinationUrl}",
                    sourceUrl,
                    target.DestinationUrl);
            }

            var properties = await GetWriteableProperties(sourceNode.Collection, cancellationToken).ConfigureAwait(false);
            return await ExecuteAsync(sourceUrl, sourceNode, target, properties, cancellationToken).ConfigureAwait(false);
        }

        private async Task<CollectionActionResult> ExecuteAsync(
            Uri sourceUrl,
            ICollectionNode sourceNode,
            TCollection target,
            IReadOnlyCollection<IUntypedWriteableProperty> properties,
            CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(
                    "Perform operation on collection {SourceUrl} with existing target {DestinationUrl}",
                    sourceUrl,
                    target.DestinationUrl);
            }

            var documentActionResults = ImmutableList<ActionResult>.Empty;
            var collectionActionResults = ImmutableList<CollectionActionResult>.Empty;

            var subNodeProperties = new Dictionary<string, IReadOnlyCollection<IUntypedWriteableProperty>>();
            foreach (var childNode in sourceNode.Nodes)
            {
                var subProperties = await GetWriteableProperties(childNode.Collection, cancellationToken).ConfigureAwait(false);
                subNodeProperties.Add(childNode.Name, subProperties);
            }

            foreach (var document in sourceNode.Documents)
            {
                var docUrl = sourceUrl.Append(document);
                if (target.Created)
                {
                    // Collection was created by us - we just assume that the document doesn't exist
                    var missingTarget = target.NewMissing(document.Name);
                    var docResult = await ExecuteAsync(docUrl, document, missingTarget, cancellationToken).ConfigureAwait(false);
                    documentActionResults = documentActionResults.Add(docResult);
                }
                else
                {
                    var foundTarget = await target.GetAsync(document.Name, cancellationToken).ConfigureAwait(false);
                    if (foundTarget is TDocument docTarget)
                    {
                        // We found a document: Business as usual when we're allowed to overwrite it
                        var docResult = await ExecuteAsync(docUrl, document, docTarget, cancellationToken).ConfigureAwait(false);
                        documentActionResults = documentActionResults.Add(docResult);
                    }
                    else
                    {
                        if (foundTarget is TCollection)
                        {
                            // We found a collection instead of a document
                            _logger.LogDebug(
                                "{DestinationUrl}: Found a collection instead of a document",
                                target.DestinationUrl);
                            var docResult = new ActionResult(ActionStatus.OverwriteFailed, foundTarget);
                            documentActionResults = documentActionResults.Add(docResult);
                        }
                        else
                        {
                            // We didn't find anything: Business as usual
                            var missingTarget = (TMissing)foundTarget;
                            var docResult = await ExecuteAsync(docUrl, document, missingTarget, cancellationToken).ConfigureAwait(false);
                            documentActionResults = documentActionResults.Add(docResult);
                        }
                    }
                }
            }

            foreach (var childNode in sourceNode.Nodes)
            {
                var childProperties = subNodeProperties[childNode.Name];
                var collection = childNode.Collection;
                var docUrl = sourceUrl.Append(childNode.Collection);
                if (target.Created)
                {
                    // Collection was created by us - we just assume that the sub collection doesn't exist
                    var missingTarget = target.NewMissing(childNode.Name);
                    var newColl = await missingTarget.CreateCollectionAsync(cancellationToken).ConfigureAwait(false);
                    var collResult = await ExecuteAsync(docUrl, childNode, newColl, childProperties, cancellationToken).ConfigureAwait(false);
                    if (collResult.Status == ActionStatus.Updated)
                    {
                        collResult = collResult with { Status = ActionStatus.Created };
                    }

                    collectionActionResults = collectionActionResults.Add(collResult);
                }
                else
                {
                    // Test if the target node exists
                    var foundTarget = await target.GetAsync(collection.Name, cancellationToken).ConfigureAwait(false);
                    if (foundTarget is TDocument)
                    {
                        // We found a document instead of a collection
                        _logger.LogDebug(
                            "{DestinationUrl}: Found a document instead of a collection",
                            target.DestinationUrl);
                        var collResult = new CollectionActionResult(ActionStatus.OverwriteFailed, foundTarget);
                        collectionActionResults = collectionActionResults.Add(collResult);
                    }
                    else
                    {
                        if (foundTarget is TCollection collTarget)
                        {
                            // We found a collection: Business as usual
                            var collResult = await ExecuteAsync(docUrl, childNode, collTarget, childProperties, cancellationToken).ConfigureAwait(false);
                            collectionActionResults = collectionActionResults.Add(collResult);
                        }
                        else
                        {
                            // We didn't find anything: Business as usual
                            var missingTarget = (TMissing)foundTarget;
                            var newColl = await missingTarget.CreateCollectionAsync(cancellationToken).ConfigureAwait(false);
                            var collResult = await ExecuteAsync(docUrl, childNode, newColl, childProperties, cancellationToken).ConfigureAwait(false);
                            collectionActionResults = collectionActionResults.Add(collResult);
                        }
                    }
                }
            }

            try
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace(
                        "Set properties on collection {DestinationUrl}",
                        target.DestinationUrl);
                }

                var failedPropertyNames = await target.SetPropertiesAsync(properties, cancellationToken).ConfigureAwait(false);
                if (failedPropertyNames.Count != 0)
                {
                    _logger.LogDebug(
                        "{DestinationUrl}: Failed setting properties {PropertyNames}",
                        target.DestinationUrl,
                        string.Join(", ", failedPropertyNames.Select(x => x.ToString())));
                    return new CollectionActionResult(ActionStatus.PropSetFailed, target)
                    {
                        FailedProperties = failedPropertyNames,
                        CollectionActionResults = collectionActionResults,
                        DocumentActionResults = documentActionResults,
                    };
                }

                var actionResults = documentActionResults.Concat(collectionActionResults);
                await _handler.CleanupAsync(
                        sourceNode.Collection,
                        target,
                        actionResults,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(
                    "{SourceCollectionPath}: Cleanup failed with exception {ErrorMessage}",
                    sourceNode.Collection.Path,
                    ex.Message);
                return new CollectionActionResult(ActionStatus.CleanupFailed, target)
                {
                    Exception = ex,
                    CollectionActionResults = collectionActionResults,
                    DocumentActionResults = documentActionResults,
                };
            }

            return new CollectionActionResult(ActionStatus.Updated, target)
            {
                CollectionActionResults = collectionActionResults,
                DocumentActionResults = documentActionResults,
            };
        }
    }
}
