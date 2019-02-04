// <copyright file="MountTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.InMemory;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Locking.InMemory;
using FubarDev.WebDavServer.Props.Store;
using FubarDev.WebDavServer.Props.Store.InMemory;
using FubarDev.WebDavServer.Tests.Support;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit;

namespace FubarDev.WebDavServer.Tests.FileSystem
{
    public class MountTests : IClassFixture<MountTests.FileSystemServices>, IDisposable
    {
        private readonly IServiceScope _serviceScope;

        public MountTests(FileSystemServices fsServices)
        {
            var serviceScopeFactory = fsServices.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
            _serviceScope = serviceScopeFactory.CreateScope();
            FileSystem = _serviceScope.ServiceProvider.GetRequiredService<IFileSystem>();
        }

        public IFileSystem FileSystem { get; }

        [Fact]
        public async Task CannotCreateDocument()
        {
            var ct = CancellationToken.None;
            var root = await FileSystem.Root.ConfigureAwait(false);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await root.CreateDocumentAsync("test1", ct).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task CannotCreateCollection()
        {
            var ct = CancellationToken.None;
            var root = await FileSystem.Root.ConfigureAwait(false);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await root.CreateCollectionAsync("test1", ct).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task CannotModifyReadOnlyEntry()
        {
            var ct = CancellationToken.None;
            var root = await FileSystem.Root.ConfigureAwait(false);
            var test = await root.GetChildAsync("test", ct);
            Assert.NotNull(test);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await test.DeleteAsync(ct).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task DocumentInMountPoint()
        {
            var ct = CancellationToken.None;
            var root = await FileSystem.Root.ConfigureAwait(false);
            var test = await root.GetChildAsync("test", ct) as ICollection;
            Assert.NotNull(test);
            var testText = await test.GetChildAsync("test.txt", ct) as IDocument;
            Assert.NotNull(testText);
            Assert.Equal("Hello!", await testText.ReadAllAsync(ct));
        }

        [Fact]
        public async Task CanRemoveDocumentInMountPoint()
        {
            var ct = CancellationToken.None;
            var root = await FileSystem.Root.ConfigureAwait(false);
            var test = await root.GetChildAsync("test", ct) as ICollection;
            Assert.NotNull(test);
            var testText = await test.GetChildAsync("test.txt", ct) as IDocument;
            Assert.NotNull(testText);
            await testText.DeleteAsync(ct).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _serviceScope.Dispose();
        }

        public class FileSystemServices : IDisposable
        {
            private readonly ServiceProvider _rootServiceProvider;
            private readonly IServiceScope _scope;

            public FileSystemServices()
            {
                IPropertyStoreFactory propertyStoreFactory = null;

                var serviceCollection = new ServiceCollection()
                    .AddOptions()
                    .AddLogging(
                        loggerBuilder =>
                        {
                            loggerBuilder
                                .AddDebug()
                                .SetMinimumLevel(LogLevel.Trace);
                        })
                    .Configure<InMemoryLockManagerOptions>(
                        opt =>
                        {
                            opt.Rounding =
                                new DefaultLockTimeRounding(DefaultLockTimeRoundingMode.OneHundredMilliseconds);
                        })
                    .AddScoped<ILockManager, InMemoryLockManager>()
                    .AddScoped<IWebDavContext>(sp => new TestHost(sp, new Uri("http://localhost/")))
                    .AddScoped<InMemoryFileSystemFactory>()
                    .AddScoped<IFileSystemFactory, MyVirtualRootFileSystemFactory>()
                    .AddScoped(
                        sp => propertyStoreFactory ?? (propertyStoreFactory =
                                  ActivatorUtilities.CreateInstance<InMemoryPropertyStoreFactory>(sp)))
                    .AddWebDav();

                _rootServiceProvider = serviceCollection.BuildServiceProvider(true);
                _scope = _rootServiceProvider.CreateScope();
            }

            public IServiceProvider ServiceProvider => _scope.ServiceProvider;

            public void Dispose()
            {
                _scope.Dispose();
                _rootServiceProvider.Dispose();
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class MyVirtualRootFileSystemFactory : InMemoryFileSystemFactory
        {
            [NotNull]
            private readonly IServiceProvider _serviceProvider;

            public MyVirtualRootFileSystemFactory(
                [NotNull] IServiceProvider serviceProvider,
                [NotNull] IPathTraversalEngine pathTraversalEngine,
                [NotNull] ISystemClock systemClock,
                ILockManager lockManager = null,
                IPropertyStoreFactory propertyStoreFactory = null)
                : base(pathTraversalEngine, systemClock, lockManager, propertyStoreFactory)
            {
                _serviceProvider = serviceProvider;
            }

            protected override void InitializeFileSystem(ICollection mountPoint, IPrincipal principal, InMemoryFileSystem fileSystem)
            {
                // Create the mount point
                var testMountPoint = fileSystem.RootCollection.CreateCollection("test");

                // Create the mount point file system
                var testMountPointFileSystemFactory = _serviceProvider.GetRequiredService<InMemoryFileSystemFactory>();
                var testMountPointFileSystem = Assert.IsType<InMemoryFileSystem>(testMountPointFileSystemFactory.CreateFileSystem(testMountPoint, principal));

                // Populate content of mount point file system
                testMountPointFileSystem.RootCollection.CreateDocument("test.txt").Data = new MemoryStream(Encoding.UTF8.GetBytes("Hello!"));

                // Add mount point
                fileSystem.Mount(testMountPoint.Path, testMountPointFileSystem);

                // Make the root file system read-only
                fileSystem.IsReadOnly = true;
            }
        }
    }
}
