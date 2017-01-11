using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    public class DotNetFileSystem : IFileSystem
    {
        public DotNetFileSystem(string rootFolder)
        {
            var root = new AsyncLazy<DotNetDirectory>(() => new DotNetDirectory(this, new DirectoryInfo(rootFolder), "/"));
            Root = new AsyncLazy<ICollection>(async () => await root);
        }

        public AsyncLazy<ICollection> Root { get; }

        public Task<SelectionResult> SelectAsync(string path, CancellationToken ct)
        {
            return new PathTraversalEngine(this).TraverseAsync(path, ct);
        }
    }
}
