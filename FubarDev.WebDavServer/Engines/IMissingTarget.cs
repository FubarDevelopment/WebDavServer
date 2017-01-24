using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines
{
    public interface IMissingTarget : ITarget
    {
        [NotNull]
        Task<ExecutionResult> CreateCollectionAsync(CancellationToken cancellationToken);

        [NotNull]
        Task<ExecutionResult> ExecuteAsync([NotNull] Uri sourceUrl, [NotNull] IDocument source, CancellationToken cancellationToken);
    }
}
