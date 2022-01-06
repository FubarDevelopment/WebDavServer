// <copyright file="MemoryLockServices.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Locking.InMemory;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Tests.Support.ServiceBuilders
{
    public class MemoryLockServices : ILockServices
    {
        public MemoryLockServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddOptions();
            serviceCollection.AddLogging(
                loggerBuilder =>
                {
                    loggerBuilder
                        .AddDebug()
                        .SetMinimumLevel(LogLevel.Trace);
                });
            serviceCollection.AddSingleton<IWebDavContextAccessor, TestWebDavContextAccessor>();
            serviceCollection.AddScoped<ISystemClock, TestSystemClock>();
            serviceCollection.Configure<InMemoryLockManagerOptions>(opt =>
            {
                opt.Rounding = new DefaultLockTimeRounding(DefaultLockTimeRoundingMode.OneHundredMilliseconds);
            });
            serviceCollection.AddTransient<ILockCleanupTask, LockCleanupTask>();
            serviceCollection.AddTransient<ILockManager, InMemoryLockManager>();
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public IServiceProvider ServiceProvider { get; }
    }
}
