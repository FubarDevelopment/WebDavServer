using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties;

namespace FubarDev.WebDavServer.Engines
{
    public class RecursiveExecutionEngine<TCollection, TDocument, TMissing>
        where TCollection : class, ICollectionTarget<TCollection, TDocument, TMissing>
        where TDocument : class, IDocumentTarget<TCollection, TDocument, TMissing>
        where TMissing : class, IMissingTarget<TCollection, TDocument, TMissing>
    {
        private readonly ITargetActions<TCollection, TDocument, TMissing> _handler;

        private readonly bool _allowOverwrite;

        public RecursiveExecutionEngine(ITargetActions<TCollection, TDocument, TMissing> handler, bool allowOverwrite)
        {
            _handler = handler;
            _allowOverwrite = allowOverwrite;
        }

        public async Task<CollectionActionResult> ExecuteAsync(Uri sourceUrl, ICollection source, Depth depth, TMissing target, CancellationToken cancellationToken)
        {
            var nodes = await source.GetNodeAsync(depth.OrderValue, cancellationToken).ConfigureAwait(false);
            return await ExecuteAsync(sourceUrl, nodes, target, cancellationToken).ConfigureAwait(false);
        }

        public async Task<CollectionActionResult> ExecuteAsync(Uri sourceUrl, ICollection source, Depth depth, TCollection target, CancellationToken cancellationToken)
        {
            var nodes = await source.GetNodeAsync(depth.OrderValue, cancellationToken).ConfigureAwait(false);
            return await ExecuteAsync(sourceUrl, nodes, target, cancellationToken).ConfigureAwait(false);
        }

        public async Task<ActionResult> ExecuteAsync(Uri sourceUrl, IDocument source, TMissing target, CancellationToken cancellationToken)
        {
            try
            {
                var newDoc = await _handler.ExecuteAsync(source, target, cancellationToken).ConfigureAwait(false);
                return new ActionResult(ActionStatus.Created, newDoc);
            }
            catch (Exception ex)
            {
                return new ActionResult(ActionStatus.CreateFailed, target)
                {
                    Exception = ex,
                };
            }
        }

        public async Task<ActionResult> ExecuteAsync(Uri sourceUrl, IDocument source, TDocument target, CancellationToken cancellationToken)
        {
            if (!_allowOverwrite)
            {
                return new ActionResult(ActionStatus.CannotOverwrite, target);
            }

            if (_handler.ExistingTargetBehaviour == RecursiveTargetBehaviour.DeleteTarget)
            {
                TMissing missingTarget;
                try
                {
                    missingTarget = await target.DeleteAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return new CollectionActionResult(ActionStatus.TargetDeleteFailed, target)
                    {
                        Exception = ex
                    };
                }

                return await ExecuteAsync(sourceUrl, source, missingTarget, cancellationToken).ConfigureAwait(false);
            }

            var properties = await source.GetProperties().OfType<IUntypedWriteableProperty>().ToList(cancellationToken).ConfigureAwait(false);

            var docActionResult = await _handler.ExecuteAsync(source, target, cancellationToken).ConfigureAwait(false);
            if (docActionResult.IsFailure)
                return docActionResult;

            var failedPropertyNames = await target.SetPropertiesAsync(properties, cancellationToken).ConfigureAwait(false);
            if (failedPropertyNames.Count != 0)
            {
                return new ActionResult(ActionStatus.PropSetFailed, target)
                {
                    FailedProperties = failedPropertyNames
                };
            }

            return new ActionResult(ActionStatus.Overwritten, target);
        }

        public async Task<CollectionActionResult> ExecuteAsync(
            Uri sourceUrl,
            CollectionExtensions.INode sourceNode,
            TMissing target,
            CancellationToken cancellationToken)
        {
            var properties = await sourceNode.Collection.GetProperties().OfType<IUntypedWriteableProperty>().ToList(cancellationToken).ConfigureAwait(false);

            TCollection newColl;
            try
            {
                newColl = await target.CreateCollectionAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new CollectionActionResult(ActionStatus.CreateFailed, target)
                {
                    Exception = ex,
                };
            }

            return await ExecuteAsync(sourceUrl, sourceNode, newColl, properties, cancellationToken).ConfigureAwait(false);
        }

        public async Task<CollectionActionResult> ExecuteAsync(
            Uri sourceUrl,
            CollectionExtensions.INode sourceNode,
            TCollection target,
            CancellationToken cancellationToken)
        {
            if (_allowOverwrite && _handler.ExistingTargetBehaviour == RecursiveTargetBehaviour.DeleteTarget)
            {
                // Only delete an existing collection when the client allows an overwrite
                TMissing missing;
                try
                {
                    missing = await target.DeleteAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return new CollectionActionResult(ActionStatus.TargetDeleteFailed, target)
                    {
                        Exception = ex
                    };
                }

                return await ExecuteAsync(sourceUrl, sourceNode, missing, cancellationToken).ConfigureAwait(false);
            }

            var properties = await sourceNode.Collection.GetProperties().OfType<IUntypedWriteableProperty>().ToList(cancellationToken).ConfigureAwait(false);
            return await ExecuteAsync(sourceUrl, sourceNode, target, properties, cancellationToken).ConfigureAwait(false);
        }

        public async Task<CollectionActionResult> ExecuteAsync(
            Uri sourceUrl,
            CollectionExtensions.INode sourceNode,
            TCollection target,
            IReadOnlyCollection<IUntypedWriteableProperty> properties,
            CancellationToken cancellationToken)
        {
            var documentActionResults = ImmutableList<ActionResult>.Empty;
            var collectionActionResults = ImmutableList<CollectionActionResult>.Empty;

            foreach (var document in sourceNode.Documents)
            {
                var docUrl = sourceUrl.Append(document);
                if (target.Created)
                {
                    // Collection was created by us - we just assume that the document doesn't exist
                    var missingTarget = target.CreateMissing(document.Name);
                    var docResult = await ExecuteAsync(docUrl, document, missingTarget, cancellationToken).ConfigureAwait(false);
                    documentActionResults = documentActionResults.Add(docResult);
                }
                else
                {
                    var foundTarget = await target.GetAsync(document.Name, cancellationToken).ConfigureAwait(false);
                    var docTarget = foundTarget as TDocument;
                    if (docTarget != null)
                    {
                        // We found a document: Business as usual when we're allowed to overwrite it
                        var docResult = await ExecuteAsync(docUrl, document, docTarget, cancellationToken).ConfigureAwait(false);
                        documentActionResults = documentActionResults.Add(docResult);
                    }
                    else
                    {
                        var collTarget = foundTarget as TCollection;
                        if (collTarget != null)
                        {
                            // We found a collection instead of a document
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
                var collection = childNode.Collection;
                var docUrl = sourceUrl.Append(childNode.Collection);
                if (target.Created)
                {
                    // Collection was created by us - we just assume that the sub collection doesn't exist
                    var missingTarget = target.CreateMissing(childNode.Name);
                    var collResult = await ExecuteAsync(docUrl, childNode, missingTarget, cancellationToken).ConfigureAwait(false);
                    collectionActionResults = collectionActionResults.Add(collResult);
                }
                else
                {
                    // Test if the target node exists
                    var foundTarget = await target.GetAsync(collection.Name, cancellationToken).ConfigureAwait(false);
                    var docTarget = foundTarget as TDocument;
                    if (docTarget != null)
                    {
                        // We found a document instead of a collection
                        var collResult = new CollectionActionResult(ActionStatus.OverwriteFailed, foundTarget);
                        collectionActionResults = collectionActionResults.Add(collResult);
                    }
                    else
                    {
                        var collTarget = foundTarget as TCollection;
                        if (collTarget != null)
                        {
                            // We found a collection: Business as usual
                            var collResult = await ExecuteAsync(docUrl, childNode, collTarget, cancellationToken).ConfigureAwait(false);
                            collectionActionResults = collectionActionResults.Add(collResult);
                        }
                        else
                        {
                            // We didn't find anything: Business as usual
                            var missingTarget = (TMissing)foundTarget;
                            var collResult = await ExecuteAsync(docUrl, childNode, missingTarget, cancellationToken).ConfigureAwait(false);
                            collectionActionResults = collectionActionResults.Add(collResult);
                        }
                    }
                }
            }

            try
            {
                var failedPropertyNames = await target.SetPropertiesAsync(properties, cancellationToken).ConfigureAwait(false);
                if (failedPropertyNames.Count != 0)
                {
                    return new CollectionActionResult(ActionStatus.PropSetFailed, target)
                    {
                        FailedProperties = failedPropertyNames,
                        CollectionActionResults = collectionActionResults,
                        DocumentActionResults = documentActionResults,
                    };
                }

                await _handler.ExecuteAsync(sourceNode.Collection, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new CollectionActionResult(ActionStatus.CleanupFailed, target)
                {
                    Exception = ex,
                    CollectionActionResults = collectionActionResults,
                    DocumentActionResults = documentActionResults,
                };
            }

            return new CollectionActionResult(ActionStatus.Created, target)
            {
                CollectionActionResults = collectionActionResults,
                DocumentActionResults = documentActionResults,
            };
        }
    }
}
