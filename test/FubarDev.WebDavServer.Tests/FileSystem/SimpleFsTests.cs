// <copyright file="SimpleFsTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Tests.Support.ServiceBuilders;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace FubarDev.WebDavServer.Tests.FileSystem
{
    public abstract class SimpleFsTests<T> : IClassFixture<T>, IDisposable
        where T : class, IFileSystemServices
    {
        private readonly IServiceScope _serviceScope;

        protected SimpleFsTests(T fsServices)
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
                    Assert.Equal(test1.Path, coll.Path);
                    Assert.Equal("test1", coll.Name);
                    Assert.NotNull(coll.Parent);
                    Assert.Equal(root.Path, coll.Parent.Path);
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
                rootChildren.OrderBy(n => n.Name),
                child =>
                {
                    Assert.NotNull(child);
                    var coll = Assert.IsAssignableFrom<ICollection>(child);
                    Assert.Equal(test1.Path, coll.Path);
                    Assert.Equal("test1", coll.Name);
                    Assert.NotNull(coll.Parent);
                    Assert.Equal(root.Path, coll.Parent.Path);
                },
                child =>
                {
                    Assert.NotNull(child);
                    var coll = Assert.IsAssignableFrom<ICollection>(child);
                    Assert.Equal(test2.Path, coll.Path);
                    Assert.Equal("test2", coll.Name);
                    Assert.NotNull(coll.Parent);
                    Assert.Equal(root.Path, coll.Parent.Path);
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
