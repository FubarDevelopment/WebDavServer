using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers
{
    public interface ICopyHandler : IClass1Handler
    {
        [NotNull, ItemNotNull]
        Task<IWebDavResult> CopyAsync([NotNull] string sourcePath, [NotNull] Uri destination, Depth depth, bool? overwrite, CancellationToken cancellationToken);
    }
}
