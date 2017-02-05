// <copyright file="GetPropertyValueAsyncDelegate.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Props
{
    public delegate Task<T> GetPropertyValueAsyncDelegate<T>(CancellationToken cancellationToken);
}
