using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Engines.DefaultTargetAction;
using FubarDev.WebDavServer.Properties;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines.RemoteTargets
{
    public class RemoteCollectionTarget : ICollectionTarget<RemoteCollectionTarget, RemoteDocumentTarget, RemoteMissingTarget>
    {
        [CanBeNull]
        private readonly RemoteCollectionTarget _parent;

        [NotNull]
        private readonly RemoteTargetActions _targetActions;

        public RemoteCollectionTarget([CanBeNull] RemoteCollectionTarget parent, [NotNull] string name, [NotNull] Uri destinationUrl, bool created, [NotNull] RemoteTargetActions targetActions)
        {
            _parent = parent;
            _targetActions = targetActions;
            Name = name;
            DestinationUrl = destinationUrl;
            Created = created;
        }

        public string Name { get; }

        public Uri DestinationUrl { get; }

        public bool Created { get; }

        public Task<IReadOnlyCollection<XName>> SetPropertiesAsync(IEnumerable<IUntypedWriteableProperty> properties, CancellationToken cancellationToken)
        {
            return _targetActions.SetPropertiesAsync(this, properties, cancellationToken);
        }

        public async Task<RemoteMissingTarget> DeleteAsync(CancellationToken cancellationToken)
        {
            await _targetActions.DeleteAsync(this, cancellationToken).ConfigureAwait(false);
            Debug.Assert(_parent != null, "_parent != null");
            return _parent.NewMissing(Name);
        }

        public Task<ITarget> GetAsync(string name, CancellationToken cancellationToken)
        {
            return _targetActions.GetAsync(name, cancellationToken);
        }

        public RemoteMissingTarget NewMissing(string name)
        {
            return new RemoteMissingTarget(this, DestinationUrl.Append(name, false), name, _targetActions);
        }
    }
}
