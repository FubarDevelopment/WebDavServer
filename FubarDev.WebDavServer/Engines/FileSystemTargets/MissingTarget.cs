using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

using JetBrains.Annotations;

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

        [NotNull]
        public static MissingTarget NewInstance(
            [NotNull] Uri destinationUrl,
            [NotNull] ICollection parent,
            [NotNull] string name,
            [NotNull] ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> targetActions)
        {
            var collUrl = new Uri(destinationUrl, new Uri(".", UriKind.Relative));
            var collTarget = new CollectionTarget(collUrl, null, parent, false, targetActions);
            var target = new MissingTarget(destinationUrl, name, collTarget, targetActions);
            return target;
        }

        public async Task<CollectionTarget> CreateCollectionAsync(CancellationToken cancellationToken)
        {
            var coll = await Parent.Collection.CreateCollectionAsync(Name, cancellationToken).ConfigureAwait(false);
            return new CollectionTarget(DestinationUrl, Parent, coll, true, _targetActions);
        }
    }
}
