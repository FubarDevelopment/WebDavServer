// <copyright file="ITypedReadableProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props
{
    /// <summary>
    /// An interface for a typed readable property
    /// </summary>
    /// <typeparam name="T">The underlying type of the <see cref="System.Xml.Linq.XElement"/></typeparam>
    public interface ITypedReadableProperty<T> : IUntypedReadableProperty
    {
        /// <summary>
        /// Gets the underlying typed value
        /// </summary>
        /// <param name="ct">The cancellation token</param>
        /// <returns>The underlying typed value</returns>
        [NotNull]
        [ItemNotNull]
        Task<T> GetValueAsync(CancellationToken ct);
    }
}
