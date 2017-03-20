// <copyright file="GetHandlerLockingTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DecaTec.WebDav;
using DecaTec.WebDav.WebDavArtifacts;

using Xunit;

namespace FubarDev.WebDavServer.Tests.Locking
{
    public class GetHandlerLockingTests : ServerTestsBase
    {
        [Fact]
        public async Task GetSucceedsAccessToLockedDocumentTest()
        {
            var response = await Client.LockAsync(
                "/test1.txt",
                WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout(),
                WebDavDepthHeaderValue.Zero,
                new LockInfo()
                {
                    LockScope = LockScope.CreateExclusiveLockScope(),
                    LockType = LockType.CreateWriteLockType(),
                }).ConfigureAwait(false);
            var prop = await WebDavResponseContentParser.ParsePropResponseContentAsync(response.EnsureSuccessStatusCode().Content).ConfigureAwait(false);
            Assert.NotNull(prop.LockDiscovery);
            Assert.Collection(
                prop.LockDiscovery.ActiveLock,
                activeLock =>
                {
                    Assert.Equal("/test1.txt", activeLock.LockRoot.Href);
                    Assert.Equal(WebDavDepthHeaderValue.Zero.ToString(), activeLock.Depth, StringComparer.OrdinalIgnoreCase);
                    Assert.IsType<Exclusive>(activeLock.LockScope.Item);
                    Assert.Null(activeLock.OwnerRaw);
                    Assert.Equal(WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout().ToString(), activeLock.Timeout, StringComparer.OrdinalIgnoreCase);
                    Assert.NotNull(activeLock.LockToken?.Href);
                    Assert.True(Uri.IsWellFormedUriString(activeLock.LockToken.Href, UriKind.RelativeOrAbsolute));
                });

            var ct = CancellationToken.None;
            var getResponse = await Client.GetAsync("/test1.txt", ct).ConfigureAwait(false);
            getResponse.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task GetAccessToLockedDocumentTest()
        {
            var response = await Client.LockAsync(
                "/test1.txt",
                WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout(),
                WebDavDepthHeaderValue.Zero,
                new LockInfo()
                {
                    LockScope = LockScope.CreateExclusiveLockScope(),
                    LockType = LockType.CreateWriteLockType(),
                }).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var lockToken = response.Headers.GetValues(WebDavRequestHeader.LockTocken).Single();
            var ct = CancellationToken.None;
            Client.DefaultRequestHeaders.Add("If", $"({lockToken})");
            var getResponse = await Client.GetAsync(
                "/test1.txt",
                ct).ConfigureAwait(false);
            getResponse.EnsureSuccessStatusCode();
        }
    }
}
