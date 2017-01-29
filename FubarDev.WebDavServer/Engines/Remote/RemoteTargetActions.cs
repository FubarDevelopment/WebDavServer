using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Properties;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines.Remote
{
    public abstract class RemoteTargetActions : ITargetActions<RemoteCollectionTarget, RemoteDocumentTarget, RemoteMissingTarget>
    {
        public abstract RecursiveTargetBehaviour ExistingTargetBehaviour { get; }

        public abstract Task<RemoteDocumentTarget> ExecuteAsync(IDocument source, RemoteMissingTarget destination, CancellationToken cancellationToken);

        public abstract Task<ActionResult> ExecuteAsync(IDocument source, RemoteDocumentTarget destination, CancellationToken cancellationToken);

        public abstract Task ExecuteAsync(ICollection source, CancellationToken cancellationToken);

        [NotNull, ItemNotNull]
        public abstract Task<IReadOnlyCollection<XName>> SetPropertiesAsync([NotNull] RemoteCollectionTarget target, [NotNull, ItemNotNull] IEnumerable<IUntypedWriteableProperty> properties, CancellationToken cancellationToken);

        [NotNull, ItemNotNull]
        public abstract Task<IReadOnlyCollection<XName>> SetPropertiesAsync([NotNull] RemoteDocumentTarget target, [NotNull, ItemNotNull] IEnumerable<IUntypedWriteableProperty> properties, CancellationToken cancellationToken);

        [NotNull, ItemNotNull]
        public abstract Task<RemoteCollectionTarget> CreateCollectionAsync([NotNull] RemoteCollectionTarget targetCollection, [NotNull] string name, CancellationToken cancellationToken);

        [NotNull, ItemNotNull]
        public abstract Task<ITarget> GetAsync([NotNull] RemoteCollectionTarget collection, [NotNull] string name, CancellationToken cancellationToken);

        [NotNull]
        public abstract Task DeleteAsync([NotNull] RemoteCollectionTarget target, CancellationToken cancellationToken);

        [NotNull]
        public abstract Task DeleteAsync([NotNull] RemoteDocumentTarget target, CancellationToken cancellationToken);
    }
}
