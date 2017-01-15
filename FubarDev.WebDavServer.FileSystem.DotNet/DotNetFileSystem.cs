using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Properties;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    public class DotNetFileSystem : IFileSystem
    {
        public DotNetFileSystem(DotNetFileSystemOptions options, string rootFolder, IPropertyStore propertyStore = null)
        {
            var root = new AsyncLazy<DotNetDirectory>(() => new DotNetDirectory(this, new DirectoryInfo(rootFolder), string.Empty));
            Root = new AsyncLazy<ICollection>(async () => await root);
            Options = options;
            PropertyStore = propertyStore;
        }

        public AsyncLazy<ICollection> Root { get; }

        public DotNetFileSystemOptions Options { get; }

        public IPropertyStore PropertyStore { get; }

        public Task<SelectionResult> SelectAsync(string path, CancellationToken ct)
        {
            return new PathTraversalEngine(this).TraverseAsync(path, ct);
        }
    }
}
