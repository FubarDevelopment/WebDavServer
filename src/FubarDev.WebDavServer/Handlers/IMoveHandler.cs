// <copyright file="IMoveHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers
{
    /// <summary>
    /// Interface for the <code>MOVE</code> handler
    /// </summary>
    public interface IMoveHandler : IClass1Handler
    {
        /// <summary>
        /// Moves from the source to the destination
        /// </summary>
        /// <param name="path">The source to move</param>
        /// <param name="destination">The destination to move to</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The result of the operation</returns>
        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> MoveAsync([NotNull] string path, [NotNull] Uri destination, CancellationToken cancellationToken);
    }
}
