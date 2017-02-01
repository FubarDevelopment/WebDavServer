using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Engines.Remote;

namespace FubarDev.WebDavServer.Sample.AspNetCore.Support
{
    public class HttpClientFactory : IRemoteHttpClientFactory
    {
        public Task<HttpClient> CreateAsync(Uri baseUrl, CancellationToken cancellationToken)
        {
            var httpClient = new HttpClient()
            {
                BaseAddress = baseUrl
            };

            return Task.FromResult(httpClient);
        }
    }
}
