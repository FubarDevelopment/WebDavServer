// <copyright file="IHttpMessageHandlerFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Engines.Remote
{
    /// <summary>
    /// The interface for a factory to create <see cref="HttpMessageHandler"/> instances.
    /// </summary>
    public interface IHttpMessageHandlerFactory
    {
        /// <summary>
        /// Creates a <see cref="HttpMessageHandler"/> for the given <paramref name="baseUrl"/>.
        /// </summary>
        /// <param name="baseUrl">The base URL to create the <see cref="HttpMessageHandler"/> for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The newly created <see cref="HttpMessageHandler"/>.</returns>
        Task<HttpMessageHandler> CreateAsync(Uri baseUrl, CancellationToken cancellationToken);
    }
}
