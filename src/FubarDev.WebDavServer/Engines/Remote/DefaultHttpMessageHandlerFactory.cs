// <copyright file="DefaultHttpMessageHandlerFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Engines.Remote
{
    public class DefaultHttpMessageHandlerFactory : IHttpMessageHandlerFactory
    {
        public Task<HttpMessageHandler> CreateAsync(Uri baseUrl, CancellationToken cancellationToken)
        {
            return Task.FromResult<HttpMessageHandler>(new HttpClientHandler());
        }
    }
}
