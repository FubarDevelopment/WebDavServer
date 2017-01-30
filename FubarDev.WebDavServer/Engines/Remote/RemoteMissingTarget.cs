using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines.Remote
{
    public class RemoteMissingTarget : IMissingTarget<RemoteCollectionTarget, RemoteDocumentTarget, RemoteMissingTarget>
    {
        [NotNull]
        private readonly RemoteTargetActions _targetActions;

        public RemoteMissingTarget([NotNull] RemoteCollectionTarget parent, [NotNull] Uri destinationUrl, [NotNull] string name, [NotNull] RemoteTargetActions targetActions)
        {
            _targetActions = targetActions;
            Parent = parent;
            Name = name;
            DestinationUrl = destinationUrl;
        }
        public string Name { get; }

        public Uri DestinationUrl { get; }

        [NotNull]
        public RemoteCollectionTarget Parent { get; }

        public Task<RemoteCollectionTarget> CreateCollectionAsync(CancellationToken cancellationToken)
        {
            return _targetActions.CreateCollectionAsync(Parent, Name, cancellationToken);
        }
    }
}
