// <copyright file="SQLiteFileSystemServices.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.SQLite;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Locking.InMemory;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Store;
using FubarDev.WebDavServer.Props.Store.InMemory;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Tests.Support.ServiceBuilders
{
    public class SQLiteFileSystemServices : IFileSystemServices, IDisposable
    {
        private readonly ConcurrentBag<string> _tempDbRootPaths = new ConcurrentBag<string>();

        public SQLiteFileSystemServices()
        {
            var serviceCollection = new ServiceCollection()
                .AddOptions()
                .AddLogging(
                    loggerBuilder =>
                    {
                        loggerBuilder
                            .AddDebug()
                            .SetMinimumLevel(LogLevel.Trace);
                    })
                .Configure<InMemoryLockManagerOptions>(opt =>
                {
                    opt.Rounding = new DefaultLockTimeRounding(DefaultLockTimeRoundingMode.OneHundredMilliseconds);
                })
                .AddSingleton<ILockManager, InMemoryLockManager>()
                .AddSingleton<IDeadPropertyFactory, DeadPropertyFactory>()
                .AddSingleton<IWebDavContextAccessor, TestWebDavContextAccessor>()
                .AddSingleton<IFileSystemFactory, SQLiteFileSystemFactory>(
                    sp =>
                    {
                        var tempRootPath = Path.Combine(
                            Path.GetTempPath(),
                            "webdavserver-sqlite-tests",
                            Guid.NewGuid().ToString("N"));
                        Directory.CreateDirectory(tempRootPath);
                        _tempDbRootPaths.Add(tempRootPath);

                        var opt = new SQLiteFileSystemOptions
                        {
                            RootPath = tempRootPath,
                        };
                        var pte = sp.GetRequiredService<IPathTraversalEngine>();
                        var psf = sp.GetService<IPropertyStoreFactory>();
                        var lm = sp.GetService<ILockManager>();

                        var fsf = new SQLiteFileSystemFactory(
                            new OptionsWrapper<SQLiteFileSystemOptions>(opt),
                            pte,
                            psf,
                            lm);
                        return fsf;
                    })
                .AddSingleton<IPropertyStoreFactory, InMemoryPropertyStoreFactory>()
                .AddWebDav();
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public IServiceProvider ServiceProvider { get; }

        public void Dispose()
        {
            foreach (var tempDbRootPath in _tempDbRootPaths.Where(Directory.Exists))
            {
                Directory.Delete(tempDbRootPath, true);
            }
        }

        private class TestWebDavContextAccessor : IWebDavContextAccessor, IDisposable
        {
            private readonly IServiceScope _serviceScope;

            public TestWebDavContextAccessor(IServiceProvider serviceProvider)
            {
                _serviceScope = serviceProvider.CreateScope();
                WebDavContext = new TestHost(_serviceScope.ServiceProvider, new Uri("http://localhost/"), (string?)null);
            }

            public IWebDavContext WebDavContext { get; }

            public void Dispose()
            {
                _serviceScope.Dispose();
            }
        }
    }
}
