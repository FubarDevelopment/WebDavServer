using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines
{
    public interface IDocumentTarget : IExistingTarget
    {
        [NotNull]
        Task<ExecutionResult> ExecuteAsync([NotNull] Uri sourceUrl, [NotNull] IDocument source, CancellationToken cancellationToken);
    }
}
