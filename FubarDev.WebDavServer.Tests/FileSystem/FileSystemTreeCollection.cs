using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.InMemory;
using FubarDev.WebDavServer.Properties.Store.InMemory;

using Xunit;

namespace FubarDev.WebDavServer.Tests.FileSystem
{
    public class FileSystemTreeCollection
    {
        [Fact]
        public async Task TreeCollectionEmpty()
        {
            var ct = CancellationToken.None;
            var fs = new InMemoryFileSystem(new PathTraversalEngine(), new InMemoryPropertyStoreFactory());
            var root = await fs.Root;
            var rootNode = await root.GetNodeAsync(int.MaxValue, ct).ConfigureAwait(false);
            Assert.Same(root, rootNode.Collection);
            Assert.Equal(0, rootNode.Documents.Count);
            Assert.Equal(0, rootNode.Nodes.Count);
        }

        [Fact]
        public async Task TreeCollectionSingleDirectory()
        {
            var ct = CancellationToken.None;
            var fs = new InMemoryFileSystem(new PathTraversalEngine(), new InMemoryPropertyStoreFactory());
            var root = await fs.Root;
            await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            var rootNode = await root.GetNodeAsync(int.MaxValue, ct).ConfigureAwait(false);
            Assert.Same(root, rootNode.Collection);
            Assert.Equal(0, rootNode.Documents.Count);
            Assert.Collection(
                rootNode.Nodes,
                node =>
                {
                    Assert.NotNull(node.Collection);
                    Assert.Equal(0, node.Documents.Count);
                    Assert.Equal(0, node.Nodes.Count);
                    Assert.Equal("test1", node.Collection.Name);
                });
        }

        private static Task<IFileSystem> CreateEmptyFileSystem(CancellationToken ct)
        {
            //var ct = CancellationToken.None;
            var fileSystem = new InMemoryFileSystem(new PathTraversalEngine(), new InMemoryPropertyStoreFactory());
            //var root = await fileSystem.Root;
            //var test1 = await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            //var test2 = await root.CreateCollectionAsync("test2", ct).ConfigureAwait(false);
            return Task.FromResult<IFileSystem>(fileSystem);
        }
    }
}
