// <copyright file="IProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props
{
    /// <summary>
    /// The base property interface
    /// </summary>
    public interface IProperty
    {
        /// <summary>
        /// Gets the XML name of the property
        /// </summary>
        [NotNull]
        XName Name { get; }

        /// <summary>
        /// Gets the language of this property value
        /// </summary>
        [CanBeNull]
        string Language { get; }

        /// <summary>
        /// Gets the alternative XML names
        /// </summary>
        [NotNull]
        [ItemNotNull]
        IReadOnlyCollection<XName> AlternativeNames { get; }
    }
}
