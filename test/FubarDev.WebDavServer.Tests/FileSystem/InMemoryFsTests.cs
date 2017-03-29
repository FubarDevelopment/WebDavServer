// <copyright file="InMemoryFsTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.InMemory;
using FubarDev.WebDavServer.Tests.Support.ServiceBuilders;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace FubarDev.WebDavServer.Tests.FileSystem
{
    public class InMemoryFsTests : IClassFixture<FileSystemServices<InMemoryFileSystemFactory>>, IDisposable
    {
        private readonly IServiceScope _serviceScope;

        public InMemoryFsTests(FileSystemServices<InMemoryFileSystemFactory> fsServices)
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
            var rootChildren = await root.GetChildrenAsync(ct).ConfigureAwait(false);
            Assert.Equal(0, rootChildren.Count);
        }

        [Fact]
        public async Task SingleEmptyDirectory()
        {
            var ct = CancellationToken.None;
            var root = await FileSystem.Root.ConfigureAwait(false);
            var test1 = await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            var rootChildren = await root.GetChildrenAsync(ct).ConfigureAwait(false);
            Assert.Collection(
                rootChildren,
                child =>
                {
                    Assert.NotNull(child);
                    var coll = Assert.IsAssignableFrom<ICollection>(child);
                    Assert.Same(test1, coll);
                    Assert.Equal("test1", coll.Name);
                    Assert.Same(root, child.Parent);
                });
        }

        [Fact]
        public async Task TwoEmptyDirectories()
        {
            var ct = CancellationToken.None;
            var root = await FileSystem.Root.ConfigureAwait(false);
            var test1 = await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            var test2 = await root.CreateCollectionAsync("test2", ct).ConfigureAwait(false);
            var rootChildren = await root.GetChildrenAsync(ct).ConfigureAwait(false);
            Assert.Collection(
                rootChildren,
                child =>
                {
                    Assert.NotNull(child);
                    var coll = Assert.IsAssignableFrom<ICollection>(child);
                    Assert.Same(test1, coll);
                    Assert.Equal("test1", coll.Name);
                    Assert.Same(root, child.Parent);
                },
                child =>
                {
                    Assert.NotNull(child);
                    var coll = Assert.IsAssignableFrom<ICollection>(child);
                    Assert.Same(test2, coll);
                    Assert.Equal("test2", coll.Name);
                    Assert.Same(root, child.Parent);
                });
        }

        [Fact]
        public async Task CannotAddTwoDirectoriesWithSameName()
        {
            var ct = CancellationToken.None;
            var root = await FileSystem.Root.ConfigureAwait(false);
            await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            await Assert.ThrowsAnyAsync<IOException>(async () => await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false)).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _serviceScope.Dispose();
        }
    }
}
