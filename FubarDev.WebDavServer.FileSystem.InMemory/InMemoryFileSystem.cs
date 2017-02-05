using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Properties.Store;

using Microsoft.VisualStudio.Threading;

namespace FubarDev.WebDavServer.FileSystem.InMemory
{
    public class InMemoryFileSystem : IFileSystem
    {
        private readonly PathTraversalEngine _pathTraversalEngine;

        public InMemoryFileSystem(PathTraversalEngine pathTraversalEngine, IPropertyStoreFactory propertyStoreFactory = null)
        {
            _pathTraversalEngine = pathTraversalEngine;
            var rootDir = new InMemoryDirectory(this, null, new Uri(string.Empty, UriKind.Relative), string.Empty);
            Root = new AsyncLazy<ICollection>(() => Task.FromResult<ICollection>(rootDir));
            PropertyStore = propertyStoreFactory?.Create(this);
        }

        public AsyncLazy<ICollection> Root { get; }

        public IPropertyStore PropertyStore { get; }

        public Task<SelectionResult> SelectAsync(string path, CancellationToken ct)
        {
            return _pathTraversalEngine.TraverseAsync(this, path, ct);
        }
    }
}
