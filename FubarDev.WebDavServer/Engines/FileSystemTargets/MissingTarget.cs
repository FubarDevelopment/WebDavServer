using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Engines.FileSystemTargets
{
    public class MissingTarget : IMissingTarget
    {
        private readonly ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> _targetActions;

        public MissingTarget(Uri destinationUrl, string name, CollectionTarget parent, ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> targetActions)
        {
            _targetActions = targetActions;
            DestinationUrl = destinationUrl;
            Name = name;
            Parent = parent;
        }

        public string Name { get; }
        public CollectionTarget Parent { get; }
        public Uri DestinationUrl { get; }

        public async Task<ExecutionResult> CreateCollectionAsync(CancellationToken cancellationToken)
        {
            var coll = await Parent.Collection.CreateCollectionAsync(Name, cancellationToken).ConfigureAwait(false);
            return new ExecutionResult()
            {
                Target = new CollectionTarget(DestinationUrl.Append("/", true), coll, _targetActions),
                Href = DestinationUrl,
                StatusCode = WebDavStatusCodes.OK
            };
        }

        public async Task<ExecutionResult> ExecuteAsync(Uri sourceUrl, IDocument source, CancellationToken cancellationToken)
        {
            var doc = await _targetActions.ExecuteAsync(source, this, cancellationToken).ConfigureAwait(false);
            return new ExecutionResult()
            {
                Target = new DocumentTarget(Parent, DestinationUrl, doc, _targetActions),
                Href = DestinationUrl,
                StatusCode = WebDavStatusCodes.OK
            };
        }
    }
}
