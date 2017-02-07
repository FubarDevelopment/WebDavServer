// <copyright file="IProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props
{
    public interface IProperty
    {
        [NotNull]
        XName Name { get; }

        [NotNull]
        [ItemNotNull]
        IReadOnlyCollection<XName> AlternativeNames { get; }
    }
}
