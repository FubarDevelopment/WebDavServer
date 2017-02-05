// <copyright file="IUntypedWriteableProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props
{
    public interface IUntypedWriteableProperty : IUntypedReadableProperty
    {
        [NotNull]
        Task SetXmlValueAsync([NotNull] XElement element, CancellationToken ct);
    }
}
