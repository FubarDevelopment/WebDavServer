// <copyright file="RemoteDocumentTarget.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Props;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines.Remote
{
    public class RemoteDocumentTarget : IDocumentTarget<RemoteCollectionTarget, RemoteDocumentTarget, RemoteMissingTarget>
    {
        [NotNull]
        private readonly RemoteTargetActions _targetActions;

        public RemoteDocumentTarget([NotNull] RemoteCollectionTarget parent, [NotNull] string name, [NotNull] Uri destinationUrl, [NotNull] RemoteTargetActions targetActions)
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

        public Task<IReadOnlyCollection<XName>> SetPropertiesAsync(IEnumerable<IUntypedWriteableProperty> properties, CancellationToken cancellationToken)
        {
            return _targetActions.SetPropertiesAsync(this, properties, cancellationToken);
        }

        public async Task<RemoteMissingTarget> DeleteAsync(CancellationToken cancellationToken)
        {
            await _targetActions.DeleteAsync(this, cancellationToken).ConfigureAwait(false);
            return Parent.NewMissing(Name);
        }
    }
}
