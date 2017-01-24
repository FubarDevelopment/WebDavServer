using System;
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
        where TCollection : ICollectionTarget<TCollection, TDocument, TMissing>
        where TDocument : IDocumentTarget<TCollection, TDocument, TMissing>
        where TMissing : IMissingTarget<TCollection, TDocument, TMissing>
    {
        private readonly ITargetActions<TCollection, TDocument, TMissing> _handler;
        private readonly bool _allowOverwrite;

        public RecursiveExecutionEngine(ITargetActions<TCollection, TDocument, TMissing> handler, bool allowOverwrite)
        {
            _handler = handler;
            _allowOverwrite = allowOverwrite;
        }

        /*
        public async Task<IImmutableList<ActionResult>> ExecuteAsync(Uri sourceUrl, ICollection source, TMissing target, CancellationToken cancellationToken)
        {
            var newColl = await target.CreateCollectionAsync(cancellationToken).ConfigureAwait(false);
            return new ExecutionResult()
            {
                Target = newColl,
                Href = newColl.DestinationUrl,
                StatusCode = WebDavStatusCodes.Created
            };
        }
        */

        /*
        public async Task<Tuple<ExecutionResult, IImmutableList<ExecutionResult>>> ExecuteAsync(Uri sourceUrl, ICollection source, TMissing target, CancellationToken cancellationToken)
        {
            var newColl = await target.CreateCollectionAsync(cancellationToken).ConfigureAwait(false);
            return new ExecutionResult()
            {
                Target = newColl,
                Href = newColl.DestinationUrl,
                StatusCode = WebDavStatusCodes.Created
            };
        }

        public async Task<Tuple<ExecutionResult, IImmutableList<ExecutionResult>>> ExecuteAsync(Uri sourceUrl, ICollection source, TCollection target, CancellationToken cancellationToken)
        {
            var result = ImmutableList<ExecutionResult>.Empty;

            if (_handler.ExistingTargetBehaviour == RecursiveTargetBehaviour.DeleteBeforeCopy)
            {
                var missingTarget = await target.DeleteAsync(cancellationToken).ConfigureAwait(false);
                var collectionResult = await ExecuteAsync(sourceUrl, source, missingTarget, cancellationToken).ConfigureAwait(false);
                if (collectionResult.IsFailure)
                {
                    result = result.Add(collectionResult);
                    return result;
                }
            }
        }
        */

        public async Task<ActionResult> ExecuteAsync(Uri sourceUrl, IDocument source, TMissing target, CancellationToken cancellationToken)
        {
            var newDoc = await _handler.ExecuteAsync(source, target, cancellationToken).ConfigureAwait(false);
            return new ActionResult()
            {
                Target = newDoc,
                Href = newDoc.DestinationUrl,
                StatusCode = WebDavStatusCodes.Created
            };
        }

        public async Task<ActionResult> ExecuteAsync(Uri sourceUrl, IDocument source, TDocument target, CancellationToken cancellationToken)
        {
            if (!_allowOverwrite)
            {
                return new ActionResult()
                {
                    Target = target,
                    Href = target.DestinationUrl,
                    StatusCode = WebDavStatusCodes.PreconditionFailed
                };
            }

            if (_handler.ExistingTargetBehaviour == RecursiveTargetBehaviour.DeleteBeforeCopy)
            {
                var missingTarget = await target.DeleteAsync(cancellationToken).ConfigureAwait(false);
                return await ExecuteAsync(sourceUrl, source, missingTarget, cancellationToken).ConfigureAwait(false);
            }

            var properties = await source.GetProperties().OfType<IUntypedWriteableProperty>().ToList(cancellationToken).ConfigureAwait(false);

            await _handler.ExecuteAsync(source, target, cancellationToken).ConfigureAwait(false);

            var failedPropertyNames = await target.SetPropertiesAsync(properties, cancellationToken).ConfigureAwait(false);
            if (failedPropertyNames.Count != 0)
            {
                var unsetProperties = $"The following properties couldn't be set: {string.Join(", ", failedPropertyNames)}";
                return new ActionResult()
                {
                    Href = target.DestinationUrl,
                    Error = new Error()
                    {
                        ItemsElementName = new[] { ItemsChoiceType.PreservedLiveProperties },
                        Items = new[] { new object() },
                    },
                    Reason = unsetProperties,
                    StatusCode = WebDavStatusCodes.Conflict
                };
            }

            return new ActionResult()
            {
                Target = target,
                Href = target.DestinationUrl,
                StatusCode = WebDavStatusCodes.NoContent,
            };
        }
    }
}
