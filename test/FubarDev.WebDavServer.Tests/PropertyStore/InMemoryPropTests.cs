// <copyright file="InMemoryPropTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.InMemory;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Store;
using FubarDev.WebDavServer.Tests.Support.ServiceBuilders;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace FubarDev.WebDavServer.Tests.PropertyStore
{
    public class InMemoryPropTests : IClassFixture<FileSystemServices<InMemoryFileSystemFactory>>, IDisposable
    {
        private readonly IServiceScope _serviceScope;

        public InMemoryPropTests(FileSystemServices<InMemoryFileSystemFactory> fsServices)
        {
            var serviceScopeFactory = fsServices.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
            _serviceScope = serviceScopeFactory.CreateScope();
            FileSystem = _serviceScope.ServiceProvider.GetRequiredService<IFileSystem>();
            Dispatcher = _serviceScope.ServiceProvider.GetRequiredService<IWebDavDispatcher>();
        }

        public IWebDavDispatcher Dispatcher { get; }

        public IFileSystem FileSystem { get; }

        [NotNull]
        public IPropertyStore PropertyStore
        {
            get
            {
                Assert.NotNull(FileSystem.PropertyStore);
                return FileSystem.PropertyStore;
            }
        }

        [Fact]
        public async Task Empty()
        {
            var ct = CancellationToken.None;
            var root = await FileSystem.Root;
            var displayNameProperty = await GetDisplayNamePropertyAsync(root, ct).ConfigureAwait(false);
            Assert.Equal(string.Empty, await displayNameProperty.GetValueAsync(ct).ConfigureAwait(false));
        }

        [Fact]
        public async Task DocumentWithExtension()
        {
            var ct = CancellationToken.None;

            var root = await FileSystem.Root;
            var doc = await root.CreateDocumentAsync("test1.txt", ct).ConfigureAwait(false);

            var displayNameProperty = await GetDisplayNamePropertyAsync(doc, ct).ConfigureAwait(false);
            Assert.Equal("test1.txt", await displayNameProperty.GetValueAsync(ct).ConfigureAwait(false));
        }

        [Fact]
        public async Task SameNameDocumentsInDifferentCollections()
        {
            var ct = CancellationToken.None;

            var root = await FileSystem.Root;
            var coll1 = await root.CreateCollectionAsync("coll1", ct).ConfigureAwait(false);
            var docRoot = await root.CreateDocumentAsync("test1.txt", ct).ConfigureAwait(false);
            var docColl1 = await coll1.CreateDocumentAsync("test1.txt", ct).ConfigureAwait(false);
            var eTagDocRoot = await PropertyStore.GetETagAsync(docRoot, ct).ConfigureAwait(false);
            var eTagDocColl1 = await PropertyStore.GetETagAsync(docColl1, ct).ConfigureAwait(false);
            Assert.NotEqual(eTagDocRoot, eTagDocColl1);
        }

        [Fact]
        public async Task DisplayNameChangeable()
        {
            var ct = CancellationToken.None;

            var root = await FileSystem.Root;
            var doc = await root.CreateDocumentAsync("test1.txt", ct).ConfigureAwait(false);
            var displayNameProperty = await GetDisplayNamePropertyAsync(doc, ct).ConfigureAwait(false);

            await displayNameProperty.SetValueAsync("test1-Dokument", ct).ConfigureAwait(false);
            Assert.Equal("test1-Dokument", await displayNameProperty.GetValueAsync(ct).ConfigureAwait(false));

            displayNameProperty = await GetDisplayNamePropertyAsync(doc, ct).ConfigureAwait(false);
            Assert.Equal("test1-Dokument", await displayNameProperty.GetValueAsync(ct).ConfigureAwait(false));
        }

        public void Dispose()
        {
            _serviceScope.Dispose();
        }

        private async Task<DisplayNameProperty> GetDisplayNamePropertyAsync(IEntry entry, CancellationToken ct)
        {
            var untypedDisplayNameProperty = await entry.GetProperties(Dispatcher).Single(x => x.Name == DisplayNameProperty.PropertyName, ct).ConfigureAwait(false);
            Assert.NotNull(untypedDisplayNameProperty);
            var displayNameProperty = Assert.IsType<DisplayNameProperty>(untypedDisplayNameProperty);
            return displayNameProperty;
        }
    }
}
