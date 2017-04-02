// <copyright file="FileSystemTreeCollection.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.InMemory;
using FubarDev.WebDavServer.Tests.Support.ServiceBuilders;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace FubarDev.WebDavServer.Tests.FileSystem
{
    public class FileSystemTreeCollection : IClassFixture<FileSystemServices<InMemoryFileSystemFactory>>, IDisposable
    {
        private readonly IServiceScope _serviceScope;

        public FileSystemTreeCollection(FileSystemServices<InMemoryFileSystemFactory> fsServices)
        {
            var serviceScopeFactory = fsServices.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
            _serviceScope = serviceScopeFactory.CreateScope();
            FileSystem = _serviceScope.ServiceProvider.GetRequiredService<IFileSystem>();
        }

        public IFileSystem FileSystem { get; }

        [Fact]
        public async Task Empty()
        {
            var ct = CancellationToken.None;
            var root = await FileSystem.Root.ConfigureAwait(false);
            var rootNode = await root.GetNodeAsync(int.MaxValue, ct).ConfigureAwait(false);
            Assert.Same(root, rootNode.Collection);
            Assert.Equal(0, rootNode.Documents.Count);
            Assert.Equal(0, rootNode.Nodes.Count);
        }

        [Fact]
        public async Task SingleEmptyDirectory()
        {
            var ct = CancellationToken.None;
            var root = await FileSystem.Root.ConfigureAwait(false);
            await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            var rootNode = await root.GetNodeAsync(int.MaxValue, ct).ConfigureAwait(false);
            Assert.Same(root, rootNode.Collection);
            Assert.Equal(0, rootNode.Documents.Count);
            Assert.Collection(
                rootNode.Nodes,
                node =>
                {
                    Assert.NotNull(node.Collection);
                    Assert.Equal("test1", node.Collection.Name);
                    Assert.Same(rootNode.Collection, node.Collection.Parent);
                    Assert.Equal(0, node.Documents.Count);
                    Assert.Equal(0, node.Nodes.Count);
                });
        }

        [Fact]
        public async Task TwoNestedEmptyDirectories()
        {
            var ct = CancellationToken.None;
            var root = await FileSystem.Root.ConfigureAwait(false);
            var test1 = await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            await test1.CreateCollectionAsync("test1.1", ct).ConfigureAwait(false);
            var rootNode = await root.GetNodeAsync(int.MaxValue, ct).ConfigureAwait(false);
            Assert.Same(root, rootNode.Collection);
            Assert.Equal(0, rootNode.Documents.Count);
            Assert.Collection(
                rootNode.Nodes,
                node1 =>
                {
                    Assert.NotNull(node1.Collection);
                    Assert.Equal("test1", node1.Collection.Name);
                    Assert.Same(rootNode.Collection, node1.Collection.Parent);
                    Assert.Equal(0, node1.Documents.Count);
                    Assert.Collection(
                        node1.Nodes,
                        node2 =>
                        {
                            Assert.NotNull(node2.Collection);
                            Assert.Equal("test1.1", node2.Collection.Name);
                            Assert.Same(node1.Collection, node2.Collection.Parent);
                            Assert.Equal(0, node2.Documents.Count);
                            Assert.Equal(0, node2.Nodes.Count);
                        });
                });
        }

        [Fact]
        public async Task TwoEmptyDirectories()
        {
            var ct = CancellationToken.None;
            var root = await FileSystem.Root.ConfigureAwait(false);
            await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            await root.CreateCollectionAsync("test2", ct).ConfigureAwait(false);
            var rootNode = await root.GetNodeAsync(int.MaxValue, ct).ConfigureAwait(false);
            Assert.Same(root, rootNode.Collection);
            Assert.Equal(0, rootNode.Documents.Count);
            Assert.Collection(
                rootNode.Nodes,
                node =>
                {
                    Assert.NotNull(node.Collection);
                    Assert.Equal("test1", node.Collection.Name);
                    Assert.Same(rootNode.Collection, node.Collection.Parent);
                    Assert.Equal(0, node.Documents.Count);
                    Assert.Equal(0, node.Nodes.Count);
                },
                node =>
                {
                    Assert.NotNull(node.Collection);
                    Assert.Equal("test2", node.Collection.Name);
                    Assert.Same(rootNode.Collection, node.Collection.Parent);
                    Assert.Equal(0, node.Documents.Count);
                    Assert.Equal(0, node.Nodes.Count);
                });
        }

        [Fact]
        public async Task TwoDirectoriesWithOneEmptyChildDirectory()
        {
            var ct = CancellationToken.None;
            var root = await FileSystem.Root.ConfigureAwait(false);
            var test1 = await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            await test1.CreateCollectionAsync("test1.1", ct).ConfigureAwait(false);
            var test2 = await root.CreateCollectionAsync("test2", ct).ConfigureAwait(false);
            await test2.CreateCollectionAsync("test2.1", ct).ConfigureAwait(false);
            var rootNode = await root.GetNodeAsync(int.MaxValue, ct).ConfigureAwait(false);
            Assert.Same(root, rootNode.Collection);
            Assert.Equal(0, rootNode.Documents.Count);
            Assert.Collection(
                rootNode.Nodes,
                node1 =>
                {
                    Assert.NotNull(node1.Collection);
                    Assert.Equal("test1", node1.Collection.Name);
                    Assert.Same(rootNode.Collection, node1.Collection.Parent);
                    Assert.Equal(0, node1.Documents.Count);
                    Assert.Collection(
                        node1.Nodes,
                        node2 =>
                        {
                            Assert.NotNull(node2.Collection);
                            Assert.Equal("test1.1", node2.Collection.Name);
                            Assert.Same(node1.Collection, node2.Collection.Parent);
                            Assert.Equal(0, node2.Documents.Count);
                            Assert.Equal(0, node2.Nodes.Count);
                        });
                },
                node1 =>
                {
                    Assert.NotNull(node1.Collection);
                    Assert.Equal("test2", node1.Collection.Name);
                    Assert.Same(rootNode.Collection, node1.Collection.Parent);
                    Assert.Equal(0, node1.Documents.Count);
                    Assert.Collection(
                        node1.Nodes,
                        node2 =>
                        {
                            Assert.NotNull(node2.Collection);
                            Assert.Equal("test2.1", node2.Collection.Name);
                            Assert.Same(node1.Collection, node2.Collection.Parent);
                            Assert.Equal(0, node2.Documents.Count);
                            Assert.Equal(0, node2.Nodes.Count);
                        });
                });
        }

        [Fact]
        public async Task TwoDirectoriesWithTwoEmptyChildDirectories()
        {
            var ct = CancellationToken.None;
            var root = await FileSystem.Root.ConfigureAwait(false);
            var test1 = await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            await test1.CreateCollectionAsync("test1.1", ct).ConfigureAwait(false);
            await test1.CreateCollectionAsync("test1.2", ct).ConfigureAwait(false);
            var test2 = await root.CreateCollectionAsync("test2", ct).ConfigureAwait(false);
            await test2.CreateCollectionAsync("test2.1", ct).ConfigureAwait(false);
            await test2.CreateCollectionAsync("test2.2", ct).ConfigureAwait(false);
            var rootNode = await root.GetNodeAsync(int.MaxValue, ct).ConfigureAwait(false);
            Assert.Same(root, rootNode.Collection);
            Assert.Equal(0, rootNode.Documents.Count);
            Assert.Collection(
                rootNode.Nodes,
                node1 =>
                {
                    Assert.NotNull(node1.Collection);
                    Assert.Equal("test1", node1.Collection.Name);
                    Assert.Same(rootNode.Collection, node1.Collection.Parent);
                    Assert.Equal(0, node1.Documents.Count);
                    Assert.Collection(
                        node1.Nodes,
                        node2 =>
                        {
                            Assert.NotNull(node2.Collection);
                            Assert.Equal("test1.1", node2.Collection.Name);
                            Assert.Same(node1.Collection, node2.Collection.Parent);
                            Assert.Equal(0, node2.Documents.Count);
                            Assert.Equal(0, node2.Nodes.Count);
                        },
                        node2 =>
                        {
                            Assert.NotNull(node2.Collection);
                            Assert.Equal("test1.2", node2.Collection.Name);
                            Assert.Same(node1.Collection, node2.Collection.Parent);
                            Assert.Equal(0, node2.Documents.Count);
                            Assert.Equal(0, node2.Nodes.Count);
                        });
                },
                node1 =>
                {
                    Assert.NotNull(node1.Collection);
                    Assert.Equal("test2", node1.Collection.Name);
                    Assert.Same(rootNode.Collection, node1.Collection.Parent);
                    Assert.Equal(0, node1.Documents.Count);
                    Assert.Collection(
                        node1.Nodes,
                        node2 =>
                        {
                            Assert.NotNull(node2.Collection);
                            Assert.Equal("test2.1", node2.Collection.Name);
                            Assert.Same(node1.Collection, node2.Collection.Parent);
                            Assert.Equal(0, node2.Documents.Count);
                            Assert.Equal(0, node2.Nodes.Count);
                        },
                        node2 =>
                        {
                            Assert.NotNull(node2.Collection);
                            Assert.Equal("test2.2", node2.Collection.Name);
                            Assert.Same(node1.Collection, node2.Collection.Parent);
                            Assert.Equal(0, node2.Documents.Count);
                            Assert.Equal(0, node2.Nodes.Count);
                        });
                });
        }

        [Fact]
        public async Task TwoDirectoriesWithTwoEmptyFiles()
        {
            var ct = CancellationToken.None;
            var root = await FileSystem.Root.ConfigureAwait(false);
            var test1 = await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            await test1.CreateDocumentAsync("test1.1", ct).ConfigureAwait(false);
            await test1.CreateDocumentAsync("test1.2", ct).ConfigureAwait(false);
            var test2 = await root.CreateCollectionAsync("test2", ct).ConfigureAwait(false);
            await test2.CreateDocumentAsync("test2.1", ct).ConfigureAwait(false);
            await test2.CreateDocumentAsync("test2.2", ct).ConfigureAwait(false);
            var rootNode = await root.GetNodeAsync(int.MaxValue, ct).ConfigureAwait(false);
            Assert.Same(root, rootNode.Collection);
            Assert.Equal(0, rootNode.Documents.Count);
            Assert.Collection(
                rootNode.Nodes,
                node1 =>
                {
                    Assert.NotNull(node1.Collection);
                    Assert.Equal("test1", node1.Collection.Name);
                    Assert.Same(rootNode.Collection, node1.Collection.Parent);
                    Assert.Equal(0, node1.Nodes.Count);
                    Assert.Collection(
                        node1.Documents,
                        document =>
                        {
                            Assert.Equal("test1.1", document.Name);
                            Assert.Same(node1.Collection, document.Parent);
                        },
                        document =>
                        {
                            Assert.Equal("test1.2", document.Name);
                            Assert.Same(node1.Collection, document.Parent);
                        });
                },
                node1 =>
                {
                    Assert.NotNull(node1.Collection);
                    Assert.Equal("test2", node1.Collection.Name);
                    Assert.Same(rootNode.Collection, node1.Collection.Parent);
                    Assert.Equal(0, node1.Nodes.Count);
                    Assert.Collection(
                        node1.Documents,
                        document =>
                        {
                            Assert.Equal("test2.1", document.Name);
                            Assert.Same(node1.Collection, document.Parent);
                        },
                        document =>
                        {
                            Assert.Equal("test2.2", document.Name);
                            Assert.Same(node1.Collection, document.Parent);
                        });
                });
        }

        [Fact]
        public async Task TwoDirectoriesWithTwoEmptyFilesAndEmptyDirectory()
        {
            var ct = CancellationToken.None;
            var root = await FileSystem.Root.ConfigureAwait(false);
            var test1 = await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            await test1.CreateDocumentAsync("test1.1", ct).ConfigureAwait(false);
            await test1.CreateCollectionAsync("test1.2", ct).ConfigureAwait(false);
            await test1.CreateDocumentAsync("test1.3", ct).ConfigureAwait(false);
            var test2 = await root.CreateCollectionAsync("test2", ct).ConfigureAwait(false);
            await test2.CreateDocumentAsync("test2.1", ct).ConfigureAwait(false);
            await test2.CreateCollectionAsync("test2.2", ct).ConfigureAwait(false);
            await test2.CreateDocumentAsync("test2.3", ct).ConfigureAwait(false);
            var rootNode = await root.GetNodeAsync(int.MaxValue, ct).ConfigureAwait(false);
            Assert.Same(root, rootNode.Collection);
            Assert.Equal(0, rootNode.Documents.Count);
            Assert.Collection(
                rootNode.Nodes,
                node1 =>
                {
                    Assert.NotNull(node1.Collection);
                    Assert.Equal("test1", node1.Collection.Name);
                    Assert.Same(rootNode.Collection, node1.Collection.Parent);
                    Assert.Collection(
                        node1.Nodes,
                        node2 =>
                        {
                            Assert.NotNull(node2.Collection);
                            Assert.Equal("test1.2", node2.Collection.Name);
                            Assert.Same(node1.Collection, node2.Collection.Parent);
                            Assert.Equal(0, node2.Documents.Count);
                            Assert.Equal(0, node2.Nodes.Count);
                        });
                    Assert.Collection(
                        node1.Documents,
                        document =>
                        {
                            Assert.Equal("test1.1", document.Name);
                            Assert.Same(node1.Collection, document.Parent);
                        },
                        document =>
                        {
                            Assert.Equal("test1.3", document.Name);
                            Assert.Same(node1.Collection, document.Parent);
                        });
                },
                node1 =>
                {
                    Assert.NotNull(node1.Collection);
                    Assert.Equal("test2", node1.Collection.Name);
                    Assert.Same(rootNode.Collection, node1.Collection.Parent);
                    Assert.Collection(
                        node1.Nodes,
                        node2 =>
                        {
                            Assert.NotNull(node2.Collection);
                            Assert.Equal("test2.2", node2.Collection.Name);
                            Assert.Same(node1.Collection, node2.Collection.Parent);
                            Assert.Equal(0, node2.Documents.Count);
                            Assert.Equal(0, node2.Nodes.Count);
                        });
                    Assert.Collection(
                        node1.Documents,
                        document =>
                        {
                            Assert.Equal("test2.1", document.Name);
                            Assert.Same(node1.Collection, document.Parent);
                        },
                        document =>
                        {
                            Assert.Equal("test2.3", document.Name);
                            Assert.Same(node1.Collection, document.Parent);
                        });
                });
        }

        public void Dispose()
        {
            _serviceScope.Dispose();
        }
    }
}
