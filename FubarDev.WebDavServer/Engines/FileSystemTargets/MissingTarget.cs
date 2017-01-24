using System;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Engines.FileSystemTargets
{
    public class MissingTarget : IMissingTarget<CollectionTarget, DocumentTarget, MissingTarget>
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

        public async Task<CollectionTarget> CreateCollectionAsync(CancellationToken cancellationToken)
        {
            var coll = await Parent.Collection.CreateCollectionAsync(Name, cancellationToken).ConfigureAwait(false);
            return new CollectionTarget(DestinationUrl, Parent, coll, _targetActions);
        }
    }
}
