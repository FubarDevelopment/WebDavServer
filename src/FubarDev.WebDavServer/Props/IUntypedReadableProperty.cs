// <copyright file="IUntypedReadableProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props
{
    /// <summary>
    /// The interface for an untyped readable property
    /// </summary>
    public interface IUntypedReadableProperty : IProperty
    {
        /// <summary>
        /// Gets the cost of reading the property value.
        /// </summary>
        int Cost { get; }

        /// <summary>
        /// Gets the <see cref="XElement"/> for this property.
        /// </summary>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The <see cref="XElement"/> for this property.</returns>
        [NotNull]
        [ItemNotNull]
        Task<XElement> GetXmlValueAsync(CancellationToken ct);
    }
}
