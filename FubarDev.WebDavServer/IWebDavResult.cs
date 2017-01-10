using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer
{
    public interface IWebDavResult
    {
        WebDavStatusCodes StatusCode { get; }

        Task WriteResponseAsync(Stream stream, CancellationToken ct);
    }
}
