// <copyright file="IWebDavResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// The result of a WebDAV operation
    /// </summary>
    public interface IWebDavResult
    {
        /// <summary>
        /// Gets the WebDAV status code.
        /// </summary>
        WebDavStatusCode StatusCode { get; }

        /// <summary>
        /// Writes the result to a <paramref name="response"/>.
        /// </summary>
        /// <param name="response">The response object to write to.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The async task.</returns>
        [NotNull]
        Task ExecuteResultAsync([NotNull] IWebDavResponse response, CancellationToken ct);
    }
}
