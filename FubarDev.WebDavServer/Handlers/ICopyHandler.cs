using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers
{
    public interface ICopyHandler : IClass1Handler
    {
        [NotNull, ItemNotNull]
        Task<IWebDavResult> CopyAsync([NotNull] string sourcePath, [NotNull] Uri destination, bool? overwrite, CancellationToken cancellationToken);
    }
}
