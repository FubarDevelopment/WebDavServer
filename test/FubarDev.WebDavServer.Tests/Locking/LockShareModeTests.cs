// <copyright file="LockShareModeTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Tests.Support.ServiceBuilders;

using LanguageExt;

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
                        true,
                        owner,
                        LockAccessType.Write,
                        LockShareMode.Exclusive,
                        TimeSpan.FromMinutes(1)),
                    ct)
                .ConfigureAwait(false);
            Assert.True(result2.IsLeft, "The second lock is expected to fail");
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
                        true,
                        owner,
                        LockAccessType.Write,
                        LockShareMode.Shared,
                        TimeSpan.FromMinutes(1)),
                    ct)
                .ConfigureAwait(false);
            Assert.True(result2.IsLeft, "The second lock is expected to fail");
        }

        private IActiveLock ValidateLockResult(Either<IReadOnlyCollection<IActiveLock>, IActiveLock> result)
        {
            if (!result.IsLeft)
                return result.RightAsEnumerable().Single();

            foreach (var activeLock in result.LeftAsEnumerable().Single())
            {
                _output.WriteLine(activeLock.ToString());
            }

            Assert.True(result.IsRight, "Conflicting locks detected");

            return result.RightAsEnumerable().Single();
        }
    }
}
