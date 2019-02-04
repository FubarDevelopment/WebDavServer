// <copyright file="DotNetFileSystemServices.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.DotNet;
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
    public class DotNetFileSystemServices : IFileSystemServices, IDisposable
    {
        private readonly ConcurrentBag<string> _tempDbRootPaths = new ConcurrentBag<string>();

        public DotNetFileSystemServices()
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
                .AddScoped<ILockManager, InMemoryLockManager>()
                .AddScoped<IDeadPropertyFactory, DeadPropertyFactory>()
                .AddScoped<IWebDavContext>(sp => new TestHost(sp, new Uri("http://localhost/")))
                .AddScoped<IFileSystemFactory>(
                    sp =>
                    {
                        var tempRootPath = Path.Combine(
                            Path.GetTempPath(),
                            "webdavserver-dotnet-tests",
                            Guid.NewGuid().ToString("N"));
                        Directory.CreateDirectory(tempRootPath);
                        _tempDbRootPaths.Add(tempRootPath);

                        var opt = new DotNetFileSystemOptions
                        {
                            RootPath = tempRootPath,
                        };
                        var pte = sp.GetRequiredService<IPathTraversalEngine>();
                        var psf = sp.GetService<IPropertyStoreFactory>();
                        var lm = sp.GetService<ILockManager>();

                        var fsf = new DotNetFileSystemFactory(
                            new OptionsWrapper<DotNetFileSystemOptions>(opt),
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
    }
}
