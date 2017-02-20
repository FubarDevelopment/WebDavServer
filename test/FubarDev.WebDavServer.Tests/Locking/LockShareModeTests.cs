// <copyright file="LockShareModeTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Tests.Support.ServiceBuilders;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Xunit.Abstractions;

namespace FubarDev.WebDavServer.Tests.Locking
{
    public class LockShareModeTests : LockTestsBase
    {
        private readonly ITestOutputHelper _output;

        public LockShareModeTests(LockServices services, ITestOutputHelper output)
            : base(services)
        {
            _output = output;
        }

        [Fact]
        public async Task TestSingleSharedAsync()
        {
            var lockManager = ServiceProvider.GetRequiredService<ILockManager>();
            var ct = CancellationToken.None;
            var owner = new XElement("test");
            var result = await lockManager
                .LockAsync(
                    new Lock(
                        "/",
                        "/",
                        true,
                        owner,
                        LockAccessType.Write,
                        LockShareMode.Shared,
                        TimeSpan.FromMinutes(1)),
                    ct)
                .ConfigureAwait(false);

            var activeLock = ValidateLockResult(result);
            Assert.NotNull(activeLock);
            Assert.Equal("/", activeLock.Path);
            Assert.True(activeLock.Recursive);
            Assert.Equal(owner, activeLock.GetOwner());
            Assert.Equal(LockAccessType.Write.Name.LocalName, activeLock.AccessType);
            Assert.Equal(LockShareMode.Shared.Name.LocalName, activeLock.ShareMode);
            Assert.Equal(TimeSpan.FromMinutes(1), activeLock.Timeout);
            Assert.True(Uri.IsWellFormedUriString(activeLock.StateToken, UriKind.RelativeOrAbsolute));
        }

        [Fact]
        public async Task TestTwoSharedAsync()
        {
            var lockManager = ServiceProvider.GetRequiredService<ILockManager>();
            var ct = CancellationToken.None;
            var owner = new XElement("test");
            var testLock = new Lock(
                "/",
                "/",
                true,
                owner,
                LockAccessType.Write,
                LockShareMode.Shared,
                TimeSpan.FromMinutes(1));
            var result1 = await lockManager.LockAsync(testLock, ct).ConfigureAwait(false);
            ValidateLockResult(result1);
            var result2 = await lockManager.LockAsync(testLock, ct).ConfigureAwait(false);
            ValidateLockResult(result2);
        }

        [Fact]
        public async Task TestSharedRootSingleExclusiveSubAsync()
        {
            var lockManager = ServiceProvider.GetRequiredService<ILockManager>();
            var ct = CancellationToken.None;
            var owner = new XElement("test");
            var result1 = await lockManager
                .LockAsync(
                    new Lock(
                        "/",
                        "/",
                        false,
                        owner,
                        LockAccessType.Write,
                        LockShareMode.Shared,
                        TimeSpan.FromMinutes(1)),
                    ct)
                .ConfigureAwait(false);
            ValidateLockResult(result1);
            var result2 = await lockManager
                .LockAsync(
                    new Lock(
                        "/test",
                        "/test",
                        true,
                        owner,
                        LockAccessType.Write,
                        LockShareMode.Exclusive,
                        TimeSpan.FromMinutes(1)),
                    ct)
                .ConfigureAwait(false);
            ValidateLockResult(result2);
        }

        [Fact]
        public async Task TestExclusiveRootSingleSharedSubAsync()
        {
            var lockManager = ServiceProvider.GetRequiredService<ILockManager>();
            var ct = CancellationToken.None;
            var owner = new XElement("test");
            var result1 = await lockManager
                .LockAsync(
                    new Lock(
                        "/",
                        "/",
                        false,
                        owner,
                        LockAccessType.Write,
                        LockShareMode.Exclusive,
                        TimeSpan.FromMinutes(1)),
                    ct)
                .ConfigureAwait(false);
            ValidateLockResult(result1);
            var result2 = await lockManager
                .LockAsync(
                    new Lock(
                        "/test",
                        "/test",
                        true,
                        owner,
                        LockAccessType.Write,
                        LockShareMode.Shared,
                        TimeSpan.FromMinutes(1)),
                    ct)
                .ConfigureAwait(false);
            ValidateLockResult(result2);
        }

        [Fact]
        public async Task TestSharedRootWithSharedSubAsync()
        {
            var lockManager = ServiceProvider.GetRequiredService<ILockManager>();
            var ct = CancellationToken.None;
            var owner = new XElement("test");
            var result1 = await lockManager
                .LockAsync(
                    new Lock(
                        "/",
                        "/",
                        true,
                        owner,
                        LockAccessType.Write,
                        LockShareMode.Shared,
                        TimeSpan.FromMinutes(1)),
                    ct)
                .ConfigureAwait(false);
            ValidateLockResult(result1);
            var result2 = await lockManager
                .LockAsync(
                    new Lock(
                        "/test",
                        "/test",
                        true,
                        owner,
                        LockAccessType.Write,
                        LockShareMode.Shared,
                        TimeSpan.FromMinutes(1)),
                    ct)
                .ConfigureAwait(false);
            ValidateLockResult(result2);
        }

        [Fact]
        public async Task TestSharedRootWithExclusiveSubAsync()
        {
            var lockManager = ServiceProvider.GetRequiredService<ILockManager>();
            var ct = CancellationToken.None;
            var owner = new XElement("test");
            var result1 = await lockManager
                .LockAsync(
                    new Lock(
                        "/",
                        "/",
                        true,
                        owner,
                        LockAccessType.Write,
                        LockShareMode.Shared,
                        TimeSpan.FromMinutes(1)),
                    ct)
                .ConfigureAwait(false);
            ValidateLockResult(result1);
            var result2 = await lockManager
                .LockAsync(
                    new Lock(
                        "/test",
                        "/test",
                        true,
                        owner,
                        LockAccessType.Write,
                        LockShareMode.Exclusive,
                        TimeSpan.FromMinutes(1)),
                    ct)
                .ConfigureAwait(false);
            Assert.Null(result2.Lock);
        }

        [Fact]
        public async Task TestExclusiveRootWithSharedSubAsync()
        {
            var lockManager = ServiceProvider.GetRequiredService<ILockManager>();
            var ct = CancellationToken.None;
            var owner = new XElement("test");
            var result1 = await lockManager
                .LockAsync(
                    new Lock(
                        "/",
                        "/",
                        true,
                        owner,
                        LockAccessType.Write,
                        LockShareMode.Exclusive,
                        TimeSpan.FromMinutes(1)),
                    ct)
                .ConfigureAwait(false);
            ValidateLockResult(result1);
            var result2 = await lockManager
                .LockAsync(
                    new Lock(
                        "/test",
                        "/test",
                        true,
                        owner,
                        LockAccessType.Write,
                        LockShareMode.Shared,
                        TimeSpan.FromMinutes(1)),
                    ct)
                .ConfigureAwait(false);
            Assert.Null(result2.Lock);
        }

        [Fact]
        public async Task TestExclusiveRootUnlockWithSharedSubAsync()
        {
            var lockManager = ServiceProvider.GetRequiredService<ILockManager>();
            var ct = CancellationToken.None;
            var owner = new XElement("test");
            var result1 = await lockManager
                .LockAsync(
                    new Lock(
                        "/",
                        "/",
                        true,
                        owner,
                        LockAccessType.Write,
                        LockShareMode.Exclusive,
                        TimeSpan.FromMinutes(1)),
                    ct)
                .ConfigureAwait(false);
            Assert.NotNull(result1.Lock);
            ValidateLockResult(result1);
            var resultRelease1 = await lockManager.ReleaseAsync(result1.Lock.Path, new Uri(result1.Lock.StateToken), ct).ConfigureAwait(false);
            Assert.Equal(LockReleaseStatus.Success, resultRelease1);
            var result2 = await lockManager
                .LockAsync(
                    new Lock(
                        "/test",
                        "/test",
                        true,
                        owner,
                        LockAccessType.Write,
                        LockShareMode.Shared,
                        TimeSpan.FromMinutes(1)),
                    ct)
                .ConfigureAwait(false);
            ValidateLockResult(result2);
        }

        [NotNull]
        private IActiveLock ValidateLockResult(LockResult result)
        {
            if (result.Lock != null)
                return result.Lock;

            Debug.Assert(result.ConflictingLocks != null, "result.ConflictingLocks != null");
            foreach (var activeLock in result.ConflictingLocks.GetLocks())
            {
                _output.WriteLine(activeLock.ToString());
            }

            throw new InvalidOperationException("Conflicting locks detected");
        }
    }
}
