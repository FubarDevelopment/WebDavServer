// <copyright file="SetPropertyValueAsyncDelegate.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Properties
{
    public delegate Task SetPropertyValueAsyncDelegate<in T>(T value, CancellationToken cancellationToken);
}
