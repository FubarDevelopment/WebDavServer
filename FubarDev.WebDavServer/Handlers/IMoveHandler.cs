// <copyright file="IMoveHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IMoveHandler : IClass1Handler
    {
        Task<IWebDavResult> MoveAsync(string path, Uri destination, bool? allowOverwrite, CancellationToken cancellationToken);
    }
}
