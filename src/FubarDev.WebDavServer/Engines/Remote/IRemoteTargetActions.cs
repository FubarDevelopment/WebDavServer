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
    public interface IRemoteTargetActions : ITargetActions<RemoteCollectionTarget, RemoteDocumentTarget, RemoteMissingTarget>, IDisposable
    {
        [NotNull]
        [ItemNotNull]
        Task<IReadOnlyCollection<XName>> SetPropertiesAsync(
            [NotNull] RemoteCollectionTarget target,
            [NotNull] [ItemNotNull] IEnumerable<IUntypedWriteableProperty> properties,
            CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<IReadOnlyCollection<XName>> SetPropertiesAsync(
            [NotNull] RemoteDocumentTarget target,
            [NotNull] [ItemNotNull] IEnumerable<IUntypedWriteableProperty> properties,
            CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<RemoteCollectionTarget> CreateCollectionAsync([NotNull] RemoteCollectionTarget targetCollection, [NotNull] string name, CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<ITarget> GetAsync([NotNull] RemoteCollectionTarget collection, [NotNull] string name, CancellationToken cancellationToken);

        [NotNull]
        Task DeleteAsync([NotNull] RemoteCollectionTarget target, CancellationToken cancellationToken);

        [NotNull]
        Task DeleteAsync([NotNull] RemoteDocumentTarget target, CancellationToken cancellationToken);
    }
}
