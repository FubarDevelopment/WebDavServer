// <copyright file="SQLiteLockServices.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.IO;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Locking.SQLite;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Tests.Support.ServiceBuilders
{
    public class SQLiteLockServices : ILockServices, IDisposable
    {
        private readonly ConcurrentBag<string> _tempDbFileNames = new ConcurrentBag<string>();

        public SQLiteLockServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddOptions();
            serviceCollection.AddLogging();
            serviceCollection.AddScoped<ISystemClock, TestSystemClock>();
            serviceCollection.AddTransient<ILockCleanupTask, LockCleanupTask>();
            serviceCollection.AddTransient<ILockManager>(
                sp =>
                {
                    var tempDbFileName = Path.GetTempFileName();
                    _tempDbFileNames.Add(tempDbFileName);
                    var config = new SQLiteLockManagerOptions()
                    {
                        Rounding = new DefaultLockTimeRounding(DefaultLockTimeRoundingMode.OneHundredMilliseconds),
                        DatabaseFileName = tempDbFileName,
                    };
                    var cleanupTask = sp.GetRequiredService<ILockCleanupTask>();
                    var systemClock = sp.GetRequiredService<ISystemClock>();
                    var logger = sp.GetRequiredService<ILogger<SQLiteLockManager>>();
                    return new SQLiteLockManager(config, cleanupTask, systemClock, logger);
                });
            ServiceProvider = serviceCollection.BuildServiceProvider();

            var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddDebug(LogLevel.Trace);
        }

        public IServiceProvider ServiceProvider { get; }

        public void Dispose()
        {
            foreach (var tempDbFileName in _tempDbFileNames)
            {
                File.Delete(tempDbFileName);
            }
        }
    }
}
