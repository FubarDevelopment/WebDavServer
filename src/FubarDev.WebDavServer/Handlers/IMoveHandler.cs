// <copyright file="IMoveHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IMoveHandler : IClass1Handler
    {
        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> MoveAsync([NotNull] string path, [NotNull] Uri destination, CancellationToken cancellationToken);
    }
}
