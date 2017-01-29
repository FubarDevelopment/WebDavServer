using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Engines.DefaultTargetAction;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines.RemoteTargets
{
    public class RemoteMissingTarget : IMissingTarget<RemoteCollectionTarget, RemoteDocumentTarget, RemoteMissingTarget>
    {
        [NotNull]
        private readonly RemoteCollectionTarget _parent;

        [NotNull]
        private readonly RemoteTargetActions _targetActions;

        public RemoteMissingTarget([NotNull] RemoteCollectionTarget parent, [NotNull] Uri destinationUrl, [NotNull] string name, [NotNull] RemoteTargetActions targetActions)
        {
            _parent = parent;
            _targetActions = targetActions;
            Name = name;
            DestinationUrl = destinationUrl;
        }

        public string Name { get; }

        public Uri DestinationUrl { get; }

        public Task<RemoteCollectionTarget> CreateCollectionAsync(CancellationToken cancellationToken)
        {
            return _targetActions.CreateCollectionAsync(_parent, Name, cancellationToken);
        }
    }
}
