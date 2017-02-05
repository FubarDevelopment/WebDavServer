// <copyright file="IProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Properties
{
    public interface IProperty
    {
        [NotNull]
        XName Name { get; }
    }
}
