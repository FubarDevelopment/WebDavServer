// <copyright file="IPropertyStore.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Props.Dead;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props.Store
{
    public interface IPropertyStore
    {
        int Cost { get; }

        [NotNull]
        [ItemCanBeNull]
        Task<XElement> GetAsync([NotNull] IEntry entry, [NotNull] XName name, CancellationToken cancellationToken);

        [NotNull]
        Task SetAsync([NotNull] IEntry entry, [NotNull] XElement element, CancellationToken cancellationToken);

        [NotNull]
        Task<bool> RemoveAsync([NotNull] IEntry entry, [NotNull] XName name, CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<IReadOnlyCollection<XElement>> GetAsync([NotNull] IEntry entry, CancellationToken cancellationToken);

        [NotNull]
        Task SetAsync([NotNull] IEntry entry, [NotNull] [ItemNotNull] IEnumerable<XElement> properties, CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<IReadOnlyCollection<bool>> RemoveAsync([NotNull] IEntry entry, [NotNull] [ItemNotNull] IEnumerable<XName> names, CancellationToken cancellationToken);

        [NotNull]
        Task RemoveAsync([NotNull] IEntry entry, CancellationToken cancellationToken);

        [NotNull]
        IDeadProperty Create([NotNull] IEntry entry, [NotNull] XName name);

        [NotNull]
        [ItemNotNull]
        Task<IDeadProperty> LoadAsync([NotNull] IEntry entry, [NotNull] XName name, CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<IReadOnlyCollection<IDeadProperty>> LoadAsync([NotNull] IEntry entry, CancellationToken cancellationToken);

        [NotNull]
        Task<EntityTag> GetETagAsync([NotNull] IEntry entry, CancellationToken cancellationToken);

        [NotNull]
        Task<EntityTag> UpdateETagAsync([NotNull] IEntry entry, CancellationToken cancellationToken);
    }
}
