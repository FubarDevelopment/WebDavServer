// <copyright file="IPropPatchHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Models;

namespace FubarDev.WebDavServer.Handlers
{
    /// <summary>
    /// Interface for the <c>PROPPATCH</c> handler
    /// </summary>
    public interface IPropPatchHandler : IClass1Handler
    {
        /// <summary>
        /// Patches (sets or removes) properties from the given <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to patch the properties for.</param>
        /// <param name="request">The properties to patch.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        Task<IWebDavResult> PropPatchAsync(string path, propertyupdate request, CancellationToken cancellationToken);
    }
}
