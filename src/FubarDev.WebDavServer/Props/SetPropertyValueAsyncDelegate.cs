// <copyright file="SetPropertyValueAsyncDelegate.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Props
{
    /// <summary>
    /// The delegate to set a typed properties value
    /// </summary>
    /// <typeparam name="T">The type of the value to set</typeparam>
    /// <param name="value">The value to set</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The async task</returns>
    public delegate Task SetPropertyValueAsyncDelegate<in T>(T value, CancellationToken cancellationToken);
}
