// <copyright file="IRemoteTargetActions.cs" company="Fubar Development Junker">
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
    /// The interface defining the remote target actions.
    /// </summary>
    public interface IRemoteTargetActions : ITargetActions<RemoteCollectionTarget, RemoteDocumentTarget, RemoteMissingTarget>, IDisposable
    {
        /// <summary>
        /// Sets the properties on a remote target.
        /// </summary>
        /// <param name="target">The remote collection target.</param>
        /// <param name="properties">The properties to set on the <paramref name="target"/>.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The list of property names that couldn't be set.</returns>
        [NotNull]
        [ItemNotNull]
        Task<IReadOnlyCollection<XName>> SetPropertiesAsync(
            [NotNull] RemoteCollectionTarget target,
            [NotNull] [ItemNotNull] IEnumerable<IUntypedWriteableProperty> properties,
            CancellationToken cancellationToken);

        /// <summary>
        /// Sets the properties on a remote target.
        /// </summary>
        /// <param name="target">The remote document target.</param>
        /// <param name="properties">The properties to set on the <paramref name="target"/>.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The list of property names that couldn't be set.</returns>
        [NotNull]
        [ItemNotNull]
        Task<IReadOnlyCollection<XName>> SetPropertiesAsync(
            [NotNull] RemoteDocumentTarget target,
            [NotNull] [ItemNotNull] IEnumerable<IUntypedWriteableProperty> properties,
            CancellationToken cancellationToken);

        /// <summary>
        /// Create a remote collection.
        /// </summary>
        /// <param name="targetCollection">The parent collection target.</param>
        /// <param name="name">The name of the new collection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The newly created remote collection target.</returns>
        [NotNull]
        [ItemNotNull]
        Task<RemoteCollectionTarget> CreateCollectionAsync([NotNull] RemoteCollectionTarget targetCollection, [NotNull] string name, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the child of a remote collection target.
        /// </summary>
        /// <param name="collection">The remote collection target to get the child target for.</param>
        /// <param name="name">The name of the child target.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The found child target (or remote missing target).</returns>
        [NotNull]
        [ItemNotNull]
        Task<ITarget> GetAsync([NotNull] RemoteCollectionTarget collection, [NotNull] string name, CancellationToken cancellationToken);

        /// <summary>
        /// Delete the remote collection target.
        /// </summary>
        /// <param name="target">The remote collection target.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The async task.</returns>
        [NotNull]
        Task DeleteAsync([NotNull] RemoteCollectionTarget target, CancellationToken cancellationToken);

        /// <summary>
        /// Delete the remote document target.
        /// </summary>
        /// <param name="target">The remote document target.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The async task.</returns>
        [NotNull]
        Task DeleteAsync([NotNull] RemoteDocumentTarget target, CancellationToken cancellationToken);
    }
}
