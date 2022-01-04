// <copyright file="SQLiteFileSystemAndPropertyServices.cs" company="Fubar Development Junker">
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
using FubarDev.WebDavServer.Props.Store.SQLite;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Tests.Support.ServiceBuilders
{
    public class SQLiteFileSystemAndPropertyServices : IFileSystemServices, IDisposable
    {
        private readonly ConcurrentBag<string> _tempDbRootPaths = new();

        public SQLiteFileSystemAndPropertyServices()
        {
            var tempRootPath = Path.Combine(
                Path.GetTempPath(),
                "webdavserver-sqlite-tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempRootPath);
            _tempDbRootPaths.Add(tempRootPath);

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
                .Configure<SQLiteFileSystemOptions>(opt => opt.RootPath = tempRootPath)
                .AddSingleton<IFileSystemFactory, SQLiteFileSystemFactory>()
                .AddSingleton<IPropertyStoreFactory, SQLitePropertyStoreFactory>()
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
