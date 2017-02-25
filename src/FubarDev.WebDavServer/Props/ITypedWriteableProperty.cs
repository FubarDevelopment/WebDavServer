// <copyright file="ITypedWriteableProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props
{
    /// <summary>
    /// A typed writeable property
    /// </summary>
    /// <typeparam name="T">The underlying type of the <see cref="System.Xml.Linq.XElement"/></typeparam>
    public interface ITypedWriteableProperty<T> : ITypedReadableProperty<T>, IUntypedWriteableProperty
    {
        /// <summary>
        /// Sets the underlying typed value
        /// </summary>
        /// <param name="value">The value to set</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns>the async task</returns>
        [NotNull]
        Task SetValueAsync([NotNull] T value, CancellationToken ct);
    }
}
