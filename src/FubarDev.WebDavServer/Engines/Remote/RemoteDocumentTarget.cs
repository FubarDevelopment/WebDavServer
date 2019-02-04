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
    /// <summary>
    /// The remote server document target.
    /// </summary>
    public class RemoteDocumentTarget : IDocumentTarget<RemoteCollectionTarget, RemoteDocumentTarget, RemoteMissingTarget>
    {
        [NotNull]
        private readonly IRemoteTargetActions _targetActions;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteDocumentTarget"/> class.
        /// </summary>
        /// <param name="parent">The parent collection.</param>
        /// <param name="name">The name of the remote document.</param>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <param name="targetActions">The target actions implementation to use.</param>
        public RemoteDocumentTarget([NotNull] RemoteCollectionTarget parent, [NotNull] string name, [NotNull] Uri destinationUrl, [NotNull] IRemoteTargetActions targetActions)
        {
            _targetActions = targetActions;
            Parent = parent;
            Name = name;
            DestinationUrl = destinationUrl;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public Uri DestinationUrl { get; }

        /// <summary>
        /// Gets the parent remote collection.
        /// </summary>
        [NotNull]
        public RemoteCollectionTarget Parent { get; }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<XName>> SetPropertiesAsync(IEnumerable<IUntypedWriteableProperty> properties, CancellationToken cancellationToken)
        {
            return _targetActions.SetPropertiesAsync(this, properties, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<RemoteMissingTarget> DeleteAsync(CancellationToken cancellationToken)
        {
            await _targetActions.DeleteAsync(this, cancellationToken).ConfigureAwait(false);
            return Parent.NewMissing(Name);
        }
    }
}
