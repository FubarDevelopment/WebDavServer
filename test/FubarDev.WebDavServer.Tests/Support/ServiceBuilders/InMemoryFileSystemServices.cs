// <copyright file="InMemoryFileSystemServices.cs" company="Fubar Development Junker">
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
    public class InMemoryFileSystemServices : IFileSystemServices
    {
        public InMemoryFileSystemServices()
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
                .AddSingleton<IFileSystemFactory, InMemoryFileSystemFactory>()
                .AddSingleton<IPropertyStoreFactory, InMemoryPropertyStoreFactory>()
                .AddWebDav();
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public IServiceProvider ServiceProvider { get; }
    }
}
