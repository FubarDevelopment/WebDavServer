// <copyright file="LockHandlerTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using WebDav;

using Xunit;

namespace FubarDev.WebDavServer.Tests.Locking
{
    public class LockHandlerTests : ServerTestsBase
    {
        [Fact]
        public async Task AddLockToRootRecursiveWithoutOwnerTest()
        {
            var ct = CancellationToken.None;
            var response = await Client.Lock(
                "/",
                new LockParameters()
                {
                    CancellationToken = ct,
                    LockScope = LockScope.Exclusive,
                    ApplyTo = ApplyTo.Lock.ResourceAndAncestors,
                }).ConfigureAwait(false);
            Assert.True(response.IsSuccessful);
            Assert.Collection(
                response.ActiveLocks,
                activeLock =>
                {
                    Assert.Equal("/", activeLock.LockRoot);
                    Assert.Equal(ApplyTo.Lock.ResourceAndAncestors, activeLock.ApplyTo);
                    Assert.Equal(LockScope.Exclusive, activeLock.LockScope);
                    Assert.Null(activeLock.Owner);
                    Assert.Null(activeLock.Timeout);
                    Assert.NotNull(activeLock.LockToken);
                    Assert.True(Uri.IsWellFormedUriString(activeLock.LockToken, UriKind.RelativeOrAbsolute));
                });
        }
    }
}
