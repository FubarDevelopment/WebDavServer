// <copyright file="GetPropertyValueAsyncDelegate.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Props
{
    /// <summary>
    /// A delegate for asynchronously getting a dead properties value.
    /// </summary>
    /// <typeparam name="T">The type of the dead properties value.</typeparam>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The underlying property value.</returns>
    public delegate Task<T> GetPropertyValueAsyncDelegate<T>(CancellationToken cancellationToken);
}
