using System;
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
        public async Task GetFailsAccessToLockedDocumentTest()
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
            var prop = await response.EnsureSuccessStatusCode()
                .Content.ParsePropResponseContentAsync().ConfigureAwait(false);
            Assert.NotNull(prop.LockDiscovery);
            Assert.Collection(
                prop.LockDiscovery.ActiveLock,
                activeLock =>
                {
                    Assert.Equal("/test1.txt", activeLock.LockRoot.Href);
                    Assert.Equal(WebDavDepthHeaderValue.Zero.ToString(), activeLock.Depth, StringComparer.OrdinalIgnoreCase);
                    Assert.IsType<Exclusive>(activeLock.LockScope.Item);
                    Assert.Null(activeLock.Owner);
                    Assert.Equal(WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout().ToString(), activeLock.Timeout, StringComparer.OrdinalIgnoreCase);
                    Assert.NotNull(activeLock.LockToken?.Href);
                    Assert.True(Uri.IsWellFormedUriString(activeLock.LockToken.Href, UriKind.RelativeOrAbsolute));
                });

            var ct = CancellationToken.None;
            var getResponse = await Client.GetAsync("/test1.txt", ct).ConfigureAwait(false);
            Assert.Equal(WebDavStatusCode.Locked, getResponse.StatusCode);
        }
    }
}
