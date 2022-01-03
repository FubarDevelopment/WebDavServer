// <copyright file="LockCleanupTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Tests.Support;
using FubarDev.WebDavServer.Tests.Support.ServiceBuilders;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace FubarDev.WebDavServer.Tests.Locking
{
    public abstract class LockCleanupTests<T> : IClassFixture<T>, IDisposable
        where T : class, ILockServices
    {
        private readonly IServiceScope _scope;

        protected LockCleanupTests(T services)
        {
            var scopeFactory = services.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
            _scope = scopeFactory.CreateScope();
        }

        private IServiceProvider ServiceProvider => _scope.ServiceProvider;

        public void Dispose()
        {
            _scope.Dispose();
        }

        [Fact]
        public async Task TestCleanupOneAsync()
        {
            var releasedLocks = new HashSet<string>();
            var systemClock = (TestSystemClock)ServiceProvider.GetRequiredService<ISystemClock>();
            var lockManager = ServiceProvider.GetRequiredService<ILockManager>();
            var ct = CancellationToken.None;

            systemClock.RoundTo(DefaultLockTimeRoundingMode.OneSecond);
            await TestSingleLockAsync(releasedLocks, lockManager, ct).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestCleanupTwoAsync()
        {
            var releasedLocks = new HashSet<string>();
            var lockManager = ServiceProvider.GetRequiredService<ILockManager>();
            var ct = CancellationToken.None;
            var owner = new XElement("test");
            var l1 = new Lock("/", "/", false, null, owner, LockAccessType.Write, LockShareMode.Shared, TimeSpan.FromMilliseconds(100));
            var l2 = new Lock("/", "/", false, null, owner, LockAccessType.Write, LockShareMode.Shared, TimeSpan.FromMilliseconds(200));
            var evt = new CountdownEvent(2);
            lockManager.LockReleased += (_, e) =>
            {
                Assert.True(releasedLocks.Add(e.Lock.StateToken));
                evt.Signal();
            };

            var systemClock = (TestSystemClock)ServiceProvider.GetRequiredService<ISystemClock>();
            systemClock.RoundTo(DefaultLockTimeRoundingMode.OneSecond);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await lockManager.LockAsync(l1, ct).ConfigureAwait(false);
            await lockManager.LockAsync(l2, ct).ConfigureAwait(false);

            Assert.True(evt.Wait(2000, ct));
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds >= 190, $"Duration should be at least 200ms, but was {stopwatch.ElapsedMilliseconds}");
        }

        [Fact]
        public async Task TestCleanupOneAfterOneAsync()
        {
            var releasedLocks = new HashSet<string>();
            var systemClock = (TestSystemClock)ServiceProvider.GetRequiredService<ISystemClock>();
            var lockManager = ServiceProvider.GetRequiredService<ILockManager>();
            var ct = CancellationToken.None;

            systemClock.RoundTo(DefaultLockTimeRoundingMode.OneSecond);
            var outerStopwatch = new Stopwatch();
            outerStopwatch.Start();

            await TestSingleLockAsync(releasedLocks, lockManager, ct).ConfigureAwait(false);
            await TestSingleLockAsync(releasedLocks, lockManager, ct).ConfigureAwait(false);

            outerStopwatch.Stop();
            Assert.True(
                outerStopwatch.ElapsedMilliseconds >= 200,
                $"Duration should be at least 200ms, but was {outerStopwatch.ElapsedMilliseconds}");
        }

        private async Task TestSingleLockAsync(ISet<string> releasedLocks, ILockManager lockManager, CancellationToken ct)
        {
            var l = new Lock(
                "/",
                "/",
                false,
                null,
                new XElement("test"),
                LockAccessType.Write,
                LockShareMode.Exclusive,
                TimeSpan.FromMilliseconds(100));
            var sem = new SemaphoreSlim(0, 1);
            var evt = new EventHandler<LockEventArgs>((_, e) =>
            {
                Assert.True(releasedLocks.Add(e.Lock.StateToken));
                sem.Release();
            });
            lockManager.LockReleased += evt;
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                await lockManager.LockAsync(l, ct).ConfigureAwait(false);
                Assert.True(await sem.WaitAsync(5000, ct).ConfigureAwait(false));
                stopwatch.Stop();
                Assert.True(
                    stopwatch.ElapsedMilliseconds >= 90,
                    $"Duration should be at least 100ms, but was {stopwatch.ElapsedMilliseconds}");
            }
            finally
            {
                lockManager.LockReleased -= evt;
            }
        }
    }
}
