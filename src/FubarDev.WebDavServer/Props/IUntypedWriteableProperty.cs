// <copyright file="IUntypedWriteableProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props
{
    /// <summary>
    /// The interface for an untyped writeable property
    /// </summary>
    public interface IUntypedWriteableProperty : IUntypedReadableProperty
    {
        /// <summary>
        /// Sets the <see cref="XElement"/> for the property.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> to be set.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The async task.</returns>
        [NotNull]
        Task SetXmlValueAsync([NotNull] XElement element, CancellationToken ct);
    }
}
