using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Properties;

namespace FubarDev.WebDavServer.FileSystem.InMemory
{
    public class InMemoryFileSystem : IFileSystem
    {
        private readonly PathTraversalEngine _pathTraversalEngine;

        public InMemoryFileSystem(PathTraversalEngine pathTraversalEngine, IPropertyStore propertyStore)
        {
            _pathTraversalEngine = pathTraversalEngine;
            Root = new AsyncLazy<ICollection>(() => new InMemoryDirectory(this, null, new Uri(string.Empty), string.Empty));
            PropertyStore = propertyStore;
        }

        public AsyncLazy<ICollection> Root { get; }
        public IPropertyStore PropertyStore { get; }

        public Task<SelectionResult> SelectAsync(string path, CancellationToken ct)
        {
            return _pathTraversalEngine.TraverseAsync(this, path, ct);
        }
    }
}
