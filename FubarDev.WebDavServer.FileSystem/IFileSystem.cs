using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface IFileSystem
    {
        ICollection Root { get; }

        Task<SelectionResult> SelectAsync([NotNull] string path, CancellationToken ct);
    }
}
