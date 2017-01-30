using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Engines.Remote
{
    public interface IRemoteHttpClientFactory
    {
        Task<HttpClient> CreateAsync(Uri baseUrl, CancellationToken cancellationToken);
    }
}
