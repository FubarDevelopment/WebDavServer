// <copyright file="CopyHandlerTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.DefaultHandlers;
using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.InMemory;
using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties.Store.InMemory;
using FubarDev.WebDavServer.Tests.Support;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Xunit;

namespace FubarDev.WebDavServer.Tests.Handlers
{
    public class CopyHandlerTests
    {
        [Fact]
        public async Task CopyFileAsync()
        {
            var ct = CancellationToken.None;
            var fileSystem = CreateFileSystem();
            await InitAsync(fileSystem, ct).ConfigureAwait(false);
            var handler = CreateHandler(fileSystem, new CopyHandlerOptions());
            var root = await fileSystem.Root.GetValueAsync(ct).ConfigureAwait(false);
            var docText1 = await root.GetChildAsync("text1.txt", ct).ConfigureAwait(false);
            Assert.NotNull(docText1);
            var docProps1 = await docText1.GetPropertyElementsAsync(ct).ConfigureAwait(false);
            await handler
                .CopyAsync("text1.txt", new Uri("text2.txt", UriKind.Relative), Depth.Zero, null, ct)
                .ConfigureAwait(false);
            var docText2 = await root.GetChildAsync("text2.txt", ct).ConfigureAwait(false);
            Assert.NotNull(docText2);
            var docProps2 = await docText2.GetPropertyElementsAsync(ct).ConfigureAwait(false);
            var changes = PropertyComparer.FindChanges(docProps1, docProps2);
            Assert.Empty(changes);
        }

        private static async Task InitAsync(IFileSystem fileSystem, CancellationToken ct)
        {
            var root = await fileSystem.Root.GetValueAsync(ct).ConfigureAwait(false);
            var doc1 = await root.CreateDocumentAsync("text1.txt", ct).ConfigureAwait(false);
            await doc1.FillWithAsync("Dokument 1", ct).ConfigureAwait(false);
            Assert.Equal("Dokument 1", await doc1.ReadAllAsync(ct).ConfigureAwait(false));
        }

        private static IFileSystem CreateFileSystem()
        {
            var fileSystem = new InMemoryFileSystem(new PathTraversalEngine(), new InMemoryPropertyStoreFactory());
            return fileSystem;
        }

        private static ICopyHandler CreateHandler(IFileSystem fileSystem, CopyHandlerOptions options)
        {
            var host = new TestHost();
            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(NullLoggerProvider.Instance);
            var logger = new Logger<CopyHandler>(loggerFactory);
            var optWrapper = new OptionsWrapper<CopyHandlerOptions>(options);
            var httpClientFactory = new HttpClientFactory();
            var handler = new CopyHandler(fileSystem, host, optWrapper, logger, httpClientFactory);
            return handler;
        }
    }
}
