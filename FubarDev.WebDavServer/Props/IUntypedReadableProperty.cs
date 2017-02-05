// <copyright file="IUntypedReadableProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props
{
    public interface IUntypedReadableProperty : IProperty
    {
        int Cost { get; }

        [NotNull]
        [ItemNotNull]
        Task<XElement> GetXmlValueAsync(CancellationToken ct);
    }
}
