// <copyright file="MoveHandlerOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Engines.Remote;

namespace FubarDev.WebDavServer.DefaultHandlers
{
    public class MoveHandlerOptions
    {
        public RecursiveProcessingMode Mode { get; set; }

        public bool OverwriteAsDefault { get; set; } = true;

        public Func<IServiceProvider, CancellationToken, Task<IRemoteMoveTargetActions>> CreateRemoteMoveTargetActionsAsync { get; set; }
    }
}
