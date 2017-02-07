// <copyright file="DefaultRemoteHttpClientFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Engines.Remote
{
    public class DefaultRemoteHttpClientFactory : IRemoteHttpClientFactory
    {
        public Task<HttpClient> CreateAsync(Uri baseUrl, CancellationToken cancellationToken)
        {
            var httpClient = new HttpClient()
            {
                BaseAddress = baseUrl,
            };

            return Task.FromResult(httpClient);
        }
    }
}
