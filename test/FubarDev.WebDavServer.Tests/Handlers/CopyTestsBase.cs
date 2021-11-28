﻿// <copyright file="CopyTestsBase.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using DecaTec.WebDav.Headers;

using FubarDev.WebDavServer.FileSystem.InMemory;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Live;
using FubarDev.WebDavServer.Tests.Support;

using Xunit;

namespace FubarDev.WebDavServer.Tests.Handlers
{
    public abstract class CopyTestsBase : ServerTestsBase
    {
        private readonly XName[] _propsToIgnoreDocument;
        private readonly XName[] _propsToIgnoreCollection;

        protected CopyTestsBase(RecursiveProcessingMode processingMode, params XName[] propertiesToIgnore)
            : base(processingMode)
        {
            _propsToIgnoreDocument = propertiesToIgnore.Union(new[] { LockDiscoveryProperty.PropertyName, DisplayNameProperty.PropertyName }).ToArray();
            _propsToIgnoreCollection = propertiesToIgnore.Union(new[] { LockDiscoveryProperty.PropertyName, DisplayNameProperty.PropertyName, GetETagProperty.PropertyName }).ToArray();
        }

        [Fact]
        public async Task CopyFileAsync()
        {
            var ct = CancellationToken.None;
            var fileSystem = GetFileSystem();
            var root = await fileSystem.Root.ConfigureAwait(false);
            var doc1 = await root.CreateDocumentAsync("text1.txt", ct).ConfigureAwait(false);
            await doc1.FillWithAsync("Document 1", ct).ConfigureAwait(false);
            Assert.Equal("Document 1", await doc1.ReadAllAsync(ct).ConfigureAwait(false));
            var props1 = await doc1.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);

            Assert.NotNull(Client.BaseAddress);
            var response = await Client
                .CopyAsync(
                    new Uri(Client.BaseAddress!, new Uri("text1.txt", UriKind.Relative)),
                    new Uri(Client.BaseAddress!, new Uri("text2.txt", UriKind.Relative)))
                .ConfigureAwait(false);
            Assert.True(response.IsSuccessStatusCode);

            var child = await root.GetChildAsync("text2.txt", ct).ConfigureAwait(false);
            var doc2 = Assert.IsType<InMemoryFile>(child);
            var props2 = await doc2.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var changes = PropertyComparer.FindChanges(props1, props2, _propsToIgnoreDocument);
            Assert.Empty(changes);
        }

        [Fact]
        public async Task CopyEmptyDirectoryAsync()
        {
            var ct = CancellationToken.None;
            var fileSystem = GetFileSystem();
            var root = await fileSystem.Root.ConfigureAwait(false);
            var coll1 = await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            Assert.NotNull(coll1);
            var props1 = await coll1.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);

            Assert.NotNull(Client.BaseAddress);
            var response = await Client
                .CopyAsync(
                    new Uri(Client.BaseAddress!, new Uri("test1", UriKind.Relative)),
                    new Uri(Client.BaseAddress!, new Uri("test2", UriKind.Relative)))
                .ConfigureAwait(false);
            Assert.True(response.IsSuccessStatusCode);

            var child = await root.GetChildAsync("test2", ct).ConfigureAwait(false);
            var coll2 = Assert.IsType<InMemoryDirectory>(child);
            var props2 = await coll2.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var changes = PropertyComparer.FindChanges(props1, props2, _propsToIgnoreCollection);
            Assert.Empty(changes);
        }

        [Fact]
        public async Task CopyDirectoryWithDocumentDepthZeroAsync()
        {
            var ct = CancellationToken.None;
            var fileSystem = GetFileSystem();
            var root = await fileSystem.Root.ConfigureAwait(false);
            var coll1 = await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            Assert.NotNull(coll1);

            var doc1 = await coll1.CreateDocumentAsync("text.txt", ct).ConfigureAwait(false);
            await doc1.FillWithAsync("Document 1", ct).ConfigureAwait(false);
            Assert.Equal("Document 1", await doc1.ReadAllAsync(ct).ConfigureAwait(false));

            var props1 = await coll1.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);

            Assert.NotNull(Client.BaseAddress);
            var response = await Client
                .CopyAsync(
                    new Uri(Client.BaseAddress!, new Uri("test1", UriKind.Relative)),
                    new Uri(Client.BaseAddress!, new Uri("test2", UriKind.Relative)),
                    true,
                    WebDavDepthHeaderValue.Zero)
                .ConfigureAwait(false);
            Assert.True(response.IsSuccessStatusCode);

            var child = await root.GetChildAsync("test2", ct).ConfigureAwait(false);
            var coll2 = Assert.IsType<InMemoryDirectory>(child);
            var props2 = await coll2.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var changes = PropertyComparer.FindChanges(props1, props2, _propsToIgnoreCollection);
            Assert.Empty(changes);

            var docChild = await coll2.GetChildAsync("text.txt", ct).ConfigureAwait(false);
            Assert.Null(docChild);
        }

        [Fact]
        public async Task CopyDirectoryWithDocumentDepthOneAsync()
        {
            var ct = CancellationToken.None;
            var fileSystem = GetFileSystem();
            var root = await fileSystem.Root.ConfigureAwait(false);
            var coll1 = await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            Assert.NotNull(coll1);

            var doc1 = await coll1.CreateDocumentAsync("text.txt", ct).ConfigureAwait(false);
            await doc1.FillWithAsync("Document 1", ct).ConfigureAwait(false);
            Assert.Equal("Document 1", await doc1.ReadAllAsync(ct).ConfigureAwait(false));

            var props1 = await coll1.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var docProps1 = await doc1.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);

            Assert.NotNull(Client.BaseAddress);
            var response = await Client
                .CopyAsync(
                    new Uri(Client.BaseAddress!, new Uri("test1", UriKind.Relative)),
                    new Uri(Client.BaseAddress!, new Uri("test2", UriKind.Relative)),
                    false,
                    WebDavDepthHeaderValue.Infinity)
                .ConfigureAwait(false);
            Assert.True(response.IsSuccessStatusCode);

            var child = await root.GetChildAsync("test2", ct).ConfigureAwait(false);
            var coll2 = Assert.IsType<InMemoryDirectory>(child);
            var props2 = await coll2.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var changes = PropertyComparer.FindChanges(props1, props2, _propsToIgnoreCollection);
            Assert.Empty(changes);

            var docChild = await coll2.GetChildAsync("text.txt", ct).ConfigureAwait(false);
            var doc2 = Assert.IsType<InMemoryFile>(docChild);
            var docProps2 = await doc2.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var docChanges = PropertyComparer.FindChanges(docProps1, docProps2, _propsToIgnoreDocument);
            Assert.Empty(docChanges);
        }

        [Fact]
        public async Task CopyDirectoryWithSubDirectoryDepthZeroAsync()
        {
            var ct = CancellationToken.None;
            var fileSystem = GetFileSystem();
            var root = await fileSystem.Root.ConfigureAwait(false);
            var coll1 = await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            Assert.NotNull(coll1);

            await coll1.CreateCollectionAsync("SubCollection", ct).ConfigureAwait(false);

            var props1 = await coll1.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);

            Assert.NotNull(Client.BaseAddress);
            var response = await Client
                .CopyAsync(
                    new Uri(Client.BaseAddress!, new Uri("test1", UriKind.Relative)),
                    new Uri(Client.BaseAddress!, new Uri("test2", UriKind.Relative)),
                    true,
                    WebDavDepthHeaderValue.Zero)
                .ConfigureAwait(false);
            Assert.True(response.IsSuccessStatusCode);

            var child = await root.GetChildAsync("test2", ct).ConfigureAwait(false);
            var coll2 = Assert.IsType<InMemoryDirectory>(child);
            var props2 = await coll2.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var changes = PropertyComparer.FindChanges(props1, props2, _propsToIgnoreCollection);
            Assert.Empty(changes);

            var subChild = await coll2.GetChildAsync("SubCollection", ct).ConfigureAwait(false);
            Assert.Null(subChild);
        }

        [Fact]
        public async Task CopyDirectoryWithSubDirectoryDepthOneAsync()
        {
            var ct = CancellationToken.None;
            var fileSystem = GetFileSystem();
            var root = await fileSystem.Root.ConfigureAwait(false);
            var coll1 = await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            Assert.NotNull(coll1);

            var sub1 = await coll1.CreateCollectionAsync("SubCollection", ct).ConfigureAwait(false);

            var props1 = await coll1.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var subProps1 = await sub1.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);

            Assert.NotNull(Client.BaseAddress);
            var response = await Client
                .CopyAsync(
                    new Uri(Client.BaseAddress!, new Uri("test1", UriKind.Relative)),
                    new Uri(Client.BaseAddress!, new Uri("test2", UriKind.Relative)),
                    true,
                    WebDavDepthHeaderValue.Infinity)
                .ConfigureAwait(false);
            Assert.True(response.IsSuccessStatusCode);

            var child = await root.GetChildAsync("test2", ct).ConfigureAwait(false);
            var coll2 = Assert.IsType<InMemoryDirectory>(child);
            var props2 = await coll2.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var changes = PropertyComparer.FindChanges(props1, props2, _propsToIgnoreCollection);
            Assert.Empty(changes);

            var subChild = await coll2.GetChildAsync("SubCollection", ct).ConfigureAwait(false);
            var sub2 = Assert.IsType<InMemoryDirectory>(subChild);
            var subProps2 = await sub2.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var subChanges = PropertyComparer.FindChanges(subProps1, subProps2, _propsToIgnoreCollection);
            Assert.Empty(subChanges);
        }

        [Fact]
        public async Task CopyDirectoryWithFileAndSubDirectoryDepthOneAsync()
        {
            var ct = CancellationToken.None;
            var fileSystem = GetFileSystem();
            var root = await fileSystem.Root.ConfigureAwait(false);
            var coll1 = await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            Assert.NotNull(coll1);

            var doc1 = await coll1.CreateDocumentAsync("text.txt", ct).ConfigureAwait(false);
            await doc1.FillWithAsync("Document 1", ct).ConfigureAwait(false);
            Assert.Equal("Document 1", await doc1.ReadAllAsync(ct).ConfigureAwait(false));

            var sub1 = await coll1.CreateCollectionAsync("SubCollection", ct).ConfigureAwait(false);

            var props1 = await coll1.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var docProps1 = await doc1.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var subProps1 = await sub1.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);

            Assert.NotNull(Client.BaseAddress);
            var response = await Client
                .CopyAsync(
                    new Uri(Client.BaseAddress!, new Uri("test1", UriKind.Relative)),
                    new Uri(Client.BaseAddress!, new Uri("test2", UriKind.Relative)),
                    true,
                    WebDavDepthHeaderValue.Infinity)
                .ConfigureAwait(false);
            Assert.True(response.IsSuccessStatusCode);

            var child = await root.GetChildAsync("test2", ct).ConfigureAwait(false);
            var coll2 = Assert.IsType<InMemoryDirectory>(child);
            var props2 = await coll2.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var changes = PropertyComparer.FindChanges(props1, props2, _propsToIgnoreCollection);
            Assert.Empty(changes);

            var docChild = await coll2.GetChildAsync("text.txt", ct).ConfigureAwait(false);
            var doc2 = Assert.IsType<InMemoryFile>(docChild);
            var docProps2 = await doc2.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var docChanges = PropertyComparer.FindChanges(docProps1, docProps2, _propsToIgnoreDocument);
            Assert.Empty(docChanges);

            var subChild = await coll2.GetChildAsync("SubCollection", ct).ConfigureAwait(false);
            var sub2 = Assert.IsType<InMemoryDirectory>(subChild);
            var subProps2 = await sub2.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var subChanges = PropertyComparer.FindChanges(subProps1, subProps2, _propsToIgnoreCollection);
            Assert.Empty(subChanges);
        }

        [Fact]
        public async Task CopyDirectoryWithFileAndSubDirectoryDepthZeroAsync()
        {
            var ct = CancellationToken.None;
            var fileSystem = GetFileSystem();
            var root = await fileSystem.Root.ConfigureAwait(false);
            var coll1 = await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            Assert.NotNull(coll1);

            var doc1 = await coll1.CreateDocumentAsync("text.txt", ct).ConfigureAwait(false);
            await doc1.FillWithAsync("Document 1", ct).ConfigureAwait(false);
            Assert.Equal("Document 1", await doc1.ReadAllAsync(ct).ConfigureAwait(false));

            await coll1.CreateCollectionAsync("SubCollection", ct).ConfigureAwait(false);

            var props1 = await coll1.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);

            Assert.NotNull(Client.BaseAddress);
            var response = await Client
                .CopyAsync(
                    new Uri(Client.BaseAddress!, new Uri("test1", UriKind.Relative)),
                    new Uri(Client.BaseAddress!, new Uri("test2", UriKind.Relative)),
                    true,
                    WebDavDepthHeaderValue.Zero)
                .ConfigureAwait(false);
            Assert.True(response.IsSuccessStatusCode);

            var child = await root.GetChildAsync("test2", ct).ConfigureAwait(false);
            var coll2 = Assert.IsType<InMemoryDirectory>(child);
            var props2 = await coll2.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var changes = PropertyComparer.FindChanges(props1, props2, _propsToIgnoreCollection);
            Assert.Empty(changes);

            var docChild = await coll2.GetChildAsync("text.txt", ct).ConfigureAwait(false);
            Assert.Null(docChild);

            var subChild = await coll2.GetChildAsync("SubCollection", ct).ConfigureAwait(false);
            Assert.Null(subChild);
        }

        [Fact]
        public async Task CopyDirectoryWithSubDirectoryAndFileDepthInfinityAsync()
        {
            var ct = CancellationToken.None;
            var fileSystem = GetFileSystem();
            var root = await fileSystem.Root.ConfigureAwait(false);
            var coll1 = await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            Assert.NotNull(coll1);

            var sub1 = await coll1.CreateCollectionAsync("SubCollection", ct).ConfigureAwait(false);

            var doc1 = await sub1.CreateDocumentAsync("text.txt", ct).ConfigureAwait(false);
            await doc1.FillWithAsync("Document 1", ct).ConfigureAwait(false);
            Assert.Equal("Document 1", await doc1.ReadAllAsync(ct).ConfigureAwait(false));

            var props1 = await coll1.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var subProps1 = await sub1.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var docProps1 = await doc1.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);

            Assert.NotNull(Client.BaseAddress);
            var response = await Client
                .CopyAsync(
                    new Uri(Client.BaseAddress!, new Uri("test1", UriKind.Relative)),
                    new Uri(Client.BaseAddress!, new Uri("test2", UriKind.Relative)),
                    true,
                    WebDavDepthHeaderValue.Infinity)
                .ConfigureAwait(false);
            Assert.True(response.IsSuccessStatusCode);

            var child = await root.GetChildAsync("test2", ct).ConfigureAwait(false);
            var coll2 = Assert.IsType<InMemoryDirectory>(child);
            var props2 = await coll2.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var changes = PropertyComparer.FindChanges(props1, props2, _propsToIgnoreCollection);
            Assert.Empty(changes);

            var subChild = await coll2.GetChildAsync("SubCollection", ct).ConfigureAwait(false);
            var sub2 = Assert.IsType<InMemoryDirectory>(subChild);
            var subProps2 = await sub2.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var subChanges = PropertyComparer.FindChanges(subProps1, subProps2, _propsToIgnoreCollection);
            Assert.Empty(subChanges);

            var docChild = await sub2.GetChildAsync("text.txt", ct).ConfigureAwait(false);
            var doc2 = Assert.IsType<InMemoryFile>(docChild);
            var docProps2 = await doc2.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var docChanges = PropertyComparer.FindChanges(docProps1, docProps2, _propsToIgnoreDocument);
            Assert.Empty(docChanges);
        }

        [Fact]
        public async Task CopyDirectoryWithTwoSubDirectoriesDepthZeroAsync()
        {
            var ct = CancellationToken.None;
            var fileSystem = GetFileSystem();
            var root = await fileSystem.Root.ConfigureAwait(false);
            var coll1 = await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            Assert.NotNull(coll1);

            await coll1.CreateCollectionAsync("subcoll1", ct).ConfigureAwait(false);
            await coll1.CreateCollectionAsync("subcoll2", ct).ConfigureAwait(false);

            var props1 = await coll1.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);

            Assert.NotNull(Client.BaseAddress);
            var response = await Client
                .CopyAsync(
                    new Uri(Client.BaseAddress!, new Uri("test1", UriKind.Relative)),
                    new Uri(Client.BaseAddress!, new Uri("test2", UriKind.Relative)),
                    true,
                    WebDavDepthHeaderValue.Zero)
                .ConfigureAwait(false);
            Assert.True(response.IsSuccessStatusCode);

            var child = await root.GetChildAsync("test2", ct).ConfigureAwait(false);
            var coll2 = Assert.IsType<InMemoryDirectory>(child);
            var props2 = await coll2.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var changes = PropertyComparer.FindChanges(props1, props2, _propsToIgnoreCollection);
            Assert.Empty(changes);

            var subChild21 = await coll2.GetChildAsync("subcoll1", ct).ConfigureAwait(false);
            Assert.Null(subChild21);

            var subChild22 = await coll2.GetChildAsync("subcoll2", ct).ConfigureAwait(false);
            Assert.Null(subChild22);
        }

        [Fact]
        public async Task CopyDirectoryWithTwoSubDirectoriesDepthOneAsync()
        {
            var ct = CancellationToken.None;
            var fileSystem = GetFileSystem();
            var root = await fileSystem.Root.ConfigureAwait(false);
            var coll1 = await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            Assert.NotNull(coll1);

            var sub11 = await coll1.CreateCollectionAsync("subcoll1", ct).ConfigureAwait(false);
            var sub12 = await coll1.CreateCollectionAsync("subcoll2", ct).ConfigureAwait(false);

            var props1 = await coll1.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var subProps11 = await sub11.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var subProps12 = await sub12.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);

            Assert.NotNull(Client.BaseAddress);
            var response = await Client
                .CopyAsync(
                    new Uri(Client.BaseAddress!, new Uri("test1", UriKind.Relative)),
                    new Uri(Client.BaseAddress!, new Uri("test2", UriKind.Relative)),
                    true,
                    WebDavDepthHeaderValue.Infinity)
                .ConfigureAwait(false);
            Assert.True(response.IsSuccessStatusCode);

            var child = await root.GetChildAsync("test2", ct).ConfigureAwait(false);
            var coll2 = Assert.IsType<InMemoryDirectory>(child);
            var props2 = await coll2.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var changes = PropertyComparer.FindChanges(props1, props2, _propsToIgnoreCollection);
            Assert.Empty(changes);

            var subChild21 = await coll2.GetChildAsync("subcoll1", ct).ConfigureAwait(false);
            var sub21 = Assert.IsType<InMemoryDirectory>(subChild21);
            var subProps21 = await sub21.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var subChanges1 = PropertyComparer.FindChanges(subProps11, subProps21, _propsToIgnoreCollection);
            Assert.Empty(subChanges1);

            var subChild22 = await coll2.GetChildAsync("subcoll2", ct).ConfigureAwait(false);
            var sub22 = Assert.IsType<InMemoryDirectory>(subChild22);
            var subProps22 = await sub22.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var subChanges2 = PropertyComparer.FindChanges(subProps12, subProps22, _propsToIgnoreCollection);
            Assert.Empty(subChanges2);
        }

        [Fact]
        public async Task CopyDirectoryWithSubDocumentAndTwoSubDirectoriesWithTwoDocumentsAsync()
        {
            var ct = CancellationToken.None;
            var fileSystem = GetFileSystem();
            var root = await fileSystem.Root.ConfigureAwait(false);

            var coll1 = await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false);
            Assert.NotNull(coll1);

            var doc1 = await coll1.CreateDocumentAsync("text.txt", ct).ConfigureAwait(false);
            await doc1.FillWithAsync("Document 1", ct).ConfigureAwait(false);
            Assert.Equal("Document 1", await doc1.ReadAllAsync(ct).ConfigureAwait(false));

            var sub11 = await coll1.CreateCollectionAsync("subcoll1", ct).ConfigureAwait(false);

            var doc111 = await sub11.CreateDocumentAsync("text11.txt", ct).ConfigureAwait(false);
            await doc111.FillWithAsync("Document 1.1.1", ct).ConfigureAwait(false);
            Assert.Equal("Document 1.1.1", await doc111.ReadAllAsync(ct).ConfigureAwait(false));

            var doc112 = await sub11.CreateDocumentAsync("text12.txt", ct).ConfigureAwait(false);
            await doc112.FillWithAsync("Document 1.1.2", ct).ConfigureAwait(false);
            Assert.Equal("Document 1.1.2", await doc112.ReadAllAsync(ct).ConfigureAwait(false));

            var sub12 = await coll1.CreateCollectionAsync("subcoll2", ct).ConfigureAwait(false);

            var doc121 = await sub12.CreateDocumentAsync("text21.txt", ct).ConfigureAwait(false);
            await doc121.FillWithAsync("Document 1.2.1", ct).ConfigureAwait(false);
            Assert.Equal("Document 1.2.1", await doc121.ReadAllAsync(ct).ConfigureAwait(false));

            var doc122 = await sub12.CreateDocumentAsync("text22.txt", ct).ConfigureAwait(false);
            await doc122.FillWithAsync("Document 1.2.2", ct).ConfigureAwait(false);
            Assert.Equal("Document 1.2.2", await doc122.ReadAllAsync(ct).ConfigureAwait(false));

            var props1 = await coll1.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var docProps1 = await doc1.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var subProps11 = await sub11.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var docProps111 = await doc111.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var docProps112 = await doc112.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var subProps12 = await sub12.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var docProps121 = await doc121.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var docProps122 = await doc122.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);

            Assert.NotNull(Client.BaseAddress);
            var response = await Client
                .CopyAsync(
                    new Uri(Client.BaseAddress!, new Uri("test1", UriKind.Relative)),
                    new Uri(Client.BaseAddress!, new Uri("test2", UriKind.Relative)),
                    true,
                    WebDavDepthHeaderValue.Infinity)
                .ConfigureAwait(false);
            Assert.True(response.IsSuccessStatusCode);

            var child = await root.GetChildAsync("test2", ct).ConfigureAwait(false);
            var coll2 = Assert.IsType<InMemoryDirectory>(child);
            var props2 = await coll2.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var changes = PropertyComparer.FindChanges(props1, props2, _propsToIgnoreCollection);
            Assert.Empty(changes);

            var docChild = await coll2.GetChildAsync("text.txt", ct).ConfigureAwait(false);
            var doc2 = Assert.IsType<InMemoryFile>(docChild);
            Assert.Equal("Document 1", await doc2.ReadAllAsync(ct).ConfigureAwait(false));
            var docProps2 = await doc2.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var docChanges = PropertyComparer.FindChanges(docProps1, docProps2, _propsToIgnoreDocument);
            Assert.Empty(docChanges);

            var subChild21 = await coll2.GetChildAsync("subcoll1", ct).ConfigureAwait(false);
            var sub21 = Assert.IsType<InMemoryDirectory>(subChild21);
            var subProps21 = await sub21.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var subChanges1 = PropertyComparer.FindChanges(subProps11, subProps21, _propsToIgnoreCollection);
            Assert.Empty(subChanges1);

            var docChild211 = await sub21.GetChildAsync("text11.txt", ct).ConfigureAwait(false);
            var doc211 = Assert.IsType<InMemoryFile>(docChild211);
            Assert.Equal("Document 1.1.1", await doc211.ReadAllAsync(ct).ConfigureAwait(false));
            var docProps211 = await doc211.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var docChanges11 = PropertyComparer.FindChanges(docProps111, docProps211, _propsToIgnoreDocument);
            Assert.Empty(docChanges11);

            var docChild212 = await sub21.GetChildAsync("text12.txt", ct).ConfigureAwait(false);
            var doc212 = Assert.IsType<InMemoryFile>(docChild212);
            Assert.Equal("Document 1.1.2", await doc212.ReadAllAsync(ct).ConfigureAwait(false));
            var docProps212 = await doc212.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var docChanges12 = PropertyComparer.FindChanges(docProps112, docProps212, _propsToIgnoreDocument);
            Assert.Empty(docChanges12);

            var subChild22 = await coll2.GetChildAsync("subcoll2", ct).ConfigureAwait(false);
            var sub22 = Assert.IsType<InMemoryDirectory>(subChild22);
            var subProps22 = await sub22.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var subChanges2 = PropertyComparer.FindChanges(subProps12, subProps22, _propsToIgnoreCollection);
            Assert.Empty(subChanges2);

            var docChild221 = await sub22.GetChildAsync("text21.txt", ct).ConfigureAwait(false);
            var doc221 = Assert.IsType<InMemoryFile>(docChild221);
            Assert.Equal("Document 1.2.1", await doc221.ReadAllAsync(ct).ConfigureAwait(false));
            var docProps221 = await doc221.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var docChanges21 = PropertyComparer.FindChanges(docProps121, docProps221, _propsToIgnoreDocument);
            Assert.Empty(docChanges21);

            var docChild222 = await sub22.GetChildAsync("text22.txt", ct).ConfigureAwait(false);
            var doc222 = Assert.IsType<InMemoryFile>(docChild222);
            Assert.Equal("Document 1.2.2", await doc222.ReadAllAsync(ct).ConfigureAwait(false));
            var docProps222 = await doc222.GetPropertyElementsAsync(DeadPropertyFactory, ct).ConfigureAwait(false);
            var docChanges22 = PropertyComparer.FindChanges(docProps122, docProps222, _propsToIgnoreDocument);
            Assert.Empty(docChanges22);
        }
    }
}
