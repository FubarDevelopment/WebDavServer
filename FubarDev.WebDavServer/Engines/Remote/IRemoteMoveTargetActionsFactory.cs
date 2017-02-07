// <copyright file="IRemoteMoveTargetActionsFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines.Remote
{
    public interface IRemoteMoveTargetActionsFactory
    {
        [NotNull]
        [ItemCanBeNull]
        Task<IRemoteMoveTargetActions> CreateMoveTargetActionsAsync(Uri destinationUrl, CancellationToken cancellationToken);
    }
}
