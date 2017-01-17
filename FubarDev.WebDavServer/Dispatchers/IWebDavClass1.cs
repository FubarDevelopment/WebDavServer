using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Dispatchers
{
    public interface IWebDavClass1 : IWebDavClass
    {
        Task<IWebDavResult> OptionsAsync(string path, CancellationToken cancellationToken);

        Task<IWebDavResult> GetAsync(string path, CancellationToken cancellationToken);

        Task<IWebDavResult> HeadAsync(string path, CancellationToken cancellationToken);

        Task<IWebDavResult> PutAsync(string path, Stream data, CancellationToken cancellationToken);

        Task<IWebDavResult> DeleteAsync(string path, CancellationToken cancellationToken);

        Task<IWebDavResult> PropFindAsync(string path, Propfind request, Depth depth, CancellationToken cancellationToken);

        Task<IWebDavResult> PropPatch(string path, Propertyupdate request, CancellationToken cancellationToken);

        Task<IWebDavResult> MkColAsync(string path, CancellationToken cancellationToken);

        Task<IWebDavResult> CopyAsync(string path, Uri destination, bool forbidOverwrite, CancellationToken cancellationToken);

        Task<IWebDavResult> MoveAsync(string path, Uri destination, bool forbidOverwrite, CancellationToken cancellationToken);
    }
}
