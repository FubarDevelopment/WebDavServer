// <copyright file="IMoveHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    /// <summary>
    /// Interface for the <c>MOVE</c> handler
    /// </summary>
    public interface IMoveHandler : IClass1Handler
    {
        /// <summary>
        /// Moves from the source to the destination.
        /// </summary>
        /// <param name="path">The source to move.</param>
        /// <param name="destination">The destination to move to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        Task<IWebDavResult> MoveAsync(string path, Uri destination, CancellationToken cancellationToken);
    }
}
