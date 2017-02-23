// <copyright file="RemoteCollectionTarget.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Props;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines.Remote
{
    /// <summary>
    /// The remote server collection target
    /// </summary>
    public class RemoteCollectionTarget : ICollectionTarget<RemoteCollectionTarget, RemoteDocumentTarget, RemoteMissingTarget>
    {
        [CanBeNull]
        private readonly RemoteCollectionTarget _parent;

        [NotNull]
        private readonly IRemoteTargetActions _targetActions;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteCollectionTarget"/> class.
        /// </summary>
        /// <param name="parent">The parent collection</param>
        /// <param name="name">The name of the remote collection</param>
        /// <param name="destinationUrl">The destination URL</param>
        /// <param name="created">Was the collection created by the <see cref="RecursiveExecutionEngine{TCollection,TDocument,TMissing}"/></param>
        /// <param name="targetActions">The target actions implementation to use</param>
        public RemoteCollectionTarget([CanBeNull] RemoteCollectionTarget parent, [NotNull] string name, [NotNull] Uri destinationUrl, bool created, [NotNull] IRemoteTargetActions targetActions)
        {
            _parent = parent;
            _targetActions = targetActions;
            Name = name;
            DestinationUrl = destinationUrl;
            Created = created;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public Uri DestinationUrl { get; }

        /// <inheritdoc />
        public bool Created { get; }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<XName>> SetPropertiesAsync(IEnumerable<IUntypedWriteableProperty> properties, CancellationToken cancellationToken)
        {
            return _targetActions.SetPropertiesAsync(this, properties, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<RemoteMissingTarget> DeleteAsync(CancellationToken cancellationToken)
        {
            await _targetActions.DeleteAsync(this, cancellationToken).ConfigureAwait(false);
            Debug.Assert(_parent != null, "_parent != null");
            return _parent.NewMissing(Name);
        }

        /// <inheritdoc />
        public Task<ITarget> GetAsync(string name, CancellationToken cancellationToken)
        {
            return _targetActions.GetAsync(this, name, cancellationToken);
        }

        /// <inheritdoc />
        public RemoteMissingTarget NewMissing(string name)
        {
            return new RemoteMissingTarget(this, DestinationUrl.Append(name, false), name, _targetActions);
        }
    }
}
