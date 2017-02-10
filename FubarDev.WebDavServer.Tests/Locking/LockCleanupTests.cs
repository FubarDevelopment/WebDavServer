// <copyright file="LockCleanupTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Locking.InMemory;
using FubarDev.WebDavServer.Tests.Support;

using Microsoft.Extensions.Options;

using Xunit;

namespace FubarDev.WebDavServer.Tests.Locking
{
    public class LockCleanupTests
    {
        private readonly TestSystemClock _systemClock;
        private readonly InMemoryLockManager _lockManager;
        private readonly LockCleanupTask _lockCleanupTask;

        public LockCleanupTests()
        {
            _systemClock = new TestSystemClock();
            var lockCleanupOptions = new LockCleanupTaskOptions()
            {
                RoundingFunc = new LockCleanupTaskOptions.DefaultRounding(
                        LockCleanupTaskOptions.DefaultRoundingMode.OneHundredMilliseconds)
                    .Round,
            };
            _lockCleanupTask = new LockCleanupTask(new OptionsWrapper<LockCleanupTaskOptions>(lockCleanupOptions), _systemClock);
            _lockManager = new InMemoryLockManager(_lockCleanupTask, _systemClock);
        }

        [Fact]
        public async Task TestCleanupSingleAsync()
        {
            var ct = CancellationToken.None;
            var l = new Lock("/", false, new XElement("test"), LockAccessType.Write, LockShareMode.Exclusive, TimeSpan.FromMilliseconds(100));
            var sem = new SemaphoreSlim(0, 1);
            _lockManager.LockAdded += (s, e) =>
            {
                sem.Release();
            };
            await _lockManager.LockAsync(l, ct).ConfigureAwait(false);
            Assert.True(await sem.WaitAsync(200, ct).ConfigureAwait(false));
        }
    }
}
