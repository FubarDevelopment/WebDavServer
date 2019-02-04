// <copyright file="IHeadHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers
{
    /// <summary>
    /// Interface for the <c>HEAD</c> handler
    /// </summary>
    public interface IHeadHandler : IClass1Handler
    {
        /// <summary>
        /// Gets the information about an element at the given path.
        /// </summary>
        /// <param name="path">The path to the element to get the information for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> HeadAsync([NotNull] string path, CancellationToken cancellationToken);
    }
}
