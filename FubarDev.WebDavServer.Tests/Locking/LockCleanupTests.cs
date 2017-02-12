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
using FubarDev.WebDavServer.Locking.InMemory;
using FubarDev.WebDavServer.Tests.Support;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace FubarDev.WebDavServer.Tests.Locking
{
    public class LockCleanupTests : LockTestsBase
    {
        public LockCleanupTests(LockServices services)
            : base(services)
        {
        }

        [Fact]
        public async Task TestCleanupOneAsync()
        {
            var lockManager = (InMemoryLockManager)ServiceProvider.GetRequiredService<ILockManager>();
            var ct = CancellationToken.None;
            var l = new Lock("/", false, new XElement("test"), LockAccessType.Write, LockShareMode.Exclusive, TimeSpan.FromMilliseconds(100));
            var sem = new SemaphoreSlim(0, 1);
            lockManager.LockReleased += (s, e) =>
            {
                sem.Release();
            };

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await lockManager.LockAsync(l, ct).ConfigureAwait(false);
            Assert.True(await sem.WaitAsync(250, ct).ConfigureAwait(false));
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds >= 100, $"Duration should be at least 100ms, but was {stopwatch.ElapsedMilliseconds}");
        }

        [Fact]
        public async Task TestCleanupTwoAsync()
        {
            var releasedLocks = new HashSet<string>();
            var lockManager = (InMemoryLockManager)ServiceProvider.GetRequiredService<ILockManager>();
            var ct = CancellationToken.None;
            var owner = new XElement("test");
            var l1 = new Lock("/", false, owner, LockAccessType.Write, LockShareMode.Shared, TimeSpan.FromMilliseconds(100));
            var l2 = new Lock("/", false, owner, LockAccessType.Write, LockShareMode.Shared, TimeSpan.FromMilliseconds(200));
            var evt = new CountdownEvent(2);
            lockManager.LockReleased += (s, e) =>
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

            Assert.True(evt.Wait(350, ct));
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds >= 200, $"Duration should be at least 200ms, but was {stopwatch.ElapsedMilliseconds}");
        }

        [Fact]
        public async Task TestCleanupOneAfterOneAsync()
        {
            var releasedLocks = new HashSet<string>();
            var systemClock = (TestSystemClock)ServiceProvider.GetRequiredService<ISystemClock>();
            var lockManager = (InMemoryLockManager)ServiceProvider.GetRequiredService<ILockManager>();
            var ct = CancellationToken.None;

            systemClock.RoundTo(DefaultLockTimeRoundingMode.OneSecond);
            var outerStopwatch = new Stopwatch();
            outerStopwatch.Start();

            {
                var l = new Lock(
                    "/",
                    false,
                    new XElement("test"),
                    LockAccessType.Write,
                    LockShareMode.Exclusive,
                    TimeSpan.FromMilliseconds(100));
                var sem = new SemaphoreSlim(0, 1);
                var evt = new EventHandler<LockEventArgs>((s, e) =>
                {
                    Assert.True(releasedLocks.Add(e.Lock.StateToken));
                    sem.Release();
                });
                lockManager.LockReleased += evt;

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                await lockManager.LockAsync(l, ct).ConfigureAwait(false);
                Assert.True(await sem.WaitAsync(250, ct).ConfigureAwait(false));
                stopwatch.Stop();
                Assert.True(
                    stopwatch.ElapsedMilliseconds >= 100,
                    $"Duration should be at least 100ms, but was {stopwatch.ElapsedMilliseconds}");

                lockManager.LockReleased -= evt;
            }

            {
                var l = new Lock(
                    "/",
                    false,
                    new XElement("test"),
                    LockAccessType.Write,
                    LockShareMode.Exclusive,
                    TimeSpan.FromMilliseconds(100));
                var sem = new SemaphoreSlim(0, 1);
                var evt = new EventHandler<LockEventArgs>((s, e) =>
                {
                    Assert.True(releasedLocks.Add(e.Lock.StateToken));
                    sem.Release();
                });
                lockManager.LockReleased += evt;

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                await lockManager.LockAsync(l, ct).ConfigureAwait(false);
                Assert.True(await sem.WaitAsync(250, ct).ConfigureAwait(false));
                stopwatch.Stop();
                Assert.True(
                    stopwatch.ElapsedMilliseconds >= 100,
                    $"Duration should be at least 100ms, but was {stopwatch.ElapsedMilliseconds}");

                lockManager.LockReleased -= evt;
            }

            outerStopwatch.Stop();
            Assert.True(
                outerStopwatch.ElapsedMilliseconds >= 200,
                $"Duration should be at least 200ms, but was {outerStopwatch.ElapsedMilliseconds}");
        }
    }
}
