using System.Net.Http;

namespace FubarDev.WebDavServer.Engines.Remote
{
    public interface IRemoteHttpClientFactory
    {
        HttpClient Create();
    }
}
