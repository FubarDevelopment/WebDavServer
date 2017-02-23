// <copyright file="RemoteMissingTarget.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines.Remote
{
    /// <summary>
    /// The missing remote target
    /// </summary>
    public class RemoteMissingTarget : IMissingTarget<RemoteCollectionTarget, RemoteDocumentTarget, RemoteMissingTarget>
    {
        [NotNull]
        private readonly IRemoteTargetActions _targetActions;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteMissingTarget"/> class.
        /// </summary>
        /// <param name="parent">The parent collection</param>
        /// <param name="destinationUrl">The destination URL</param>
        /// <param name="name">The name of the missing remote targe</param>
        /// <param name="targetActions">The target actions implementation to use</param>
        public RemoteMissingTarget([NotNull] RemoteCollectionTarget parent, [NotNull] Uri destinationUrl, [NotNull] string name, [NotNull] IRemoteTargetActions targetActions)
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
        /// Gets the parent remote collection
        /// </summary>
        [NotNull]
        public RemoteCollectionTarget Parent { get; }

        /// <inheritdoc />
        public Task<RemoteCollectionTarget> CreateCollectionAsync(CancellationToken cancellationToken)
        {
            return _targetActions.CreateCollectionAsync(Parent, Name, cancellationToken);
        }
    }
}
