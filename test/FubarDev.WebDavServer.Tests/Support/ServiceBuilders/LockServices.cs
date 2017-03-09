// <copyright file="LockServices.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Locking.InMemory;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Tests.Support.ServiceBuilders
{
    public class LockServices
    {
        public LockServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddOptions();
            serviceCollection.AddLogging();
            serviceCollection.AddScoped<ISystemClock, TestSystemClock>();
            serviceCollection.Configure<InMemoryLockManagerOptions>(opt =>
            {
                opt.Rounding = new DefaultLockTimeRounding(DefaultLockTimeRoundingMode.OneHundredMilliseconds);
            });
            serviceCollection.AddTransient<LockCleanupTask>();
            serviceCollection.AddTransient<ILockManager, InMemoryLockManager>();
            ServiceProvider = serviceCollection.BuildServiceProvider();

            var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddDebug(LogLevel.Trace);
        }

        public IServiceProvider ServiceProvider { get; }
    }
}
