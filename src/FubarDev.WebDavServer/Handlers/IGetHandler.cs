// <copyright file="IGetHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    /// <summary>
    /// Interface for the <c>GET</c> handler
    /// </summary>
    public interface IGetHandler : IClass1Handler
    {
        /// <summary>
        /// Gets the element at the given path.
        /// </summary>
        /// <param name="path">The path to the element to get.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        Task<IWebDavResult> GetAsync(string path, CancellationToken cancellationToken);
    }
}
