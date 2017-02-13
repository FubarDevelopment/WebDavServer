// <copyright file="IHttpMessageHandlerFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Engines.Remote
{
    public interface IHttpMessageHandlerFactory
    {
        Task<HttpMessageHandler> CreateAsync(Uri baseUrl, CancellationToken cancellationToken);
    }
}
