using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Properties;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    public class DotNetFileSystem : IFileSystem
    {
        private readonly PathTraversalEngine _pathTraversalEngine;

        public DotNetFileSystem(DotNetFileSystemOptions options, string rootFolder, PathTraversalEngine pathTraversalEngine, IPropertyStore propertyStore = null)
        {
            _pathTraversalEngine = pathTraversalEngine;
            var root = new AsyncLazy<DotNetDirectory>(() => new DotNetDirectory(this, new DirectoryInfo(rootFolder), new Uri(string.Empty, UriKind.Relative)));
            Root = new AsyncLazy<ICollection>(async () => await root);
            Options = options;
            PropertyStore = propertyStore;
        }

        public bool AllowInfiniteDepth => Options.AllowInfiniteDepth;

        public AsyncLazy<ICollection> Root { get; }

        public DotNetFileSystemOptions Options { get; }

        public IPropertyStore PropertyStore { get; }

        public Task<SelectionResult> SelectAsync(string path, CancellationToken ct)
        {
            return _pathTraversalEngine.TraverseAsync(this, path, ct);
        }
    }
}
