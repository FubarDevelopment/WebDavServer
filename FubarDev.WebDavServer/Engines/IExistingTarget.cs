// <copyright file="IExistingTarget.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Properties;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines
{
    public interface IExistingTarget : ITarget
    {
        [NotNull]
        Task<IReadOnlyCollection<XName>> SetPropertiesAsync(IEnumerable<IUntypedWriteableProperty> properties, CancellationToken cancellationToken);
    }
}
