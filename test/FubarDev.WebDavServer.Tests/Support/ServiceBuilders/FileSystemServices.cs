// <copyright file="FileSystemServices.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.InMemory;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Locking.InMemory;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Store;
using FubarDev.WebDavServer.Props.Store.InMemory;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Tests.Support.ServiceBuilders
{
    public class FileSystemServices
    {
        public FileSystemServices()
        {
            var serviceCollection = new ServiceCollection()
                .AddOptions()
                .AddLogging()
                .AddScoped<ISystemClock, TestSystemClock>()
                .Configure<LockManagerOptions>(opt =>
                {
                    opt.Rounding = new DefaultLockTimeRounding(DefaultLockTimeRoundingMode.OneHundredMilliseconds);
                })
                .AddScoped<LockCleanupTask>()
                .AddScoped<ILockManager, InMemoryLockManager>()
                .AddScoped<IDeadPropertyFactory, DeadPropertyFactory>()
                .AddTransient<PathTraversalEngine>()
                .AddScoped<IWebDavContext>(sp => new TestHost(new Uri("http://localhost/")))
                .AddScoped<IFileSystemFactory, InMemoryFileSystemFactory>()
                .AddSingleton<IPropertyStoreFactory, InMemoryPropertyStoreFactory>()
                .AddScoped(ctx =>
                {
                    var factory = ctx.GetRequiredService<IFileSystemFactory>();
                    var webDavContext = ctx.GetRequiredService<IWebDavContext>();
                    return factory.CreateFileSystem(webDavContext.User.Identity);
                })
                .AddScoped(ctx =>
                {
                    var factory = ctx.GetRequiredService<IPropertyStoreFactory>();
                    var fs = ctx.GetRequiredService<IFileSystem>();
                    return factory.Create(fs);
                });
            ServiceProvider = serviceCollection.BuildServiceProvider();

            var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddDebug(LogLevel.Trace);
        }

        public IServiceProvider ServiceProvider { get; }
    }
}
