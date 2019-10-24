// <copyright file="IPropFindHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Handlers
{
    /// <summary>
    /// Interface for the <c>PROPFIND</c> handler
    /// </summary>
    public interface IPropFindHandler : IClass1Handler
    {
        /// <summary>
        /// Queries properties (dead or live) for a given <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to query the properties for.</param>
        /// <param name="request">Some information about the properties to query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        Task<IWebDavResult> PropFindAsync(string path, propfind? request, CancellationToken cancellationToken);
    }
}
